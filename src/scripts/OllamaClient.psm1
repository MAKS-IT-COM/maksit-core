<#
.SYNOPSIS
    Generic Ollama API client module for PowerShell.

.DESCRIPTION
    Provides a simple interface to interact with Ollama's local LLM API:
    - Text generation (chat/completion)
    - Embeddings generation
    - Model management
    - RAG utilities (cosine similarity, clustering)

.REQUIREMENTS
    - Ollama running locally (default: http://localhost:11434)

.USAGE
    Import-Module .\OllamaClient.psm1
    
    # Configure
    Set-OllamaConfig -ApiUrl "http://localhost:11434"
    
    # Check availability
    if (Test-OllamaAvailable) {
        # Generate text
        $response = Invoke-OllamaPrompt -Model "llama3.1:8b" -Prompt "Hello!"
        
        # Get embeddings
        $embedding = Get-OllamaEmbedding -Model "nomic-embed-text" -Text "Sample text"
    }
#>

# ==============================================================================
# MODULE CONFIGURATION
# ==============================================================================

$script:OllamaConfig = @{
    ApiUrl = "http://localhost:11434"
    DefaultTimeout = 180
    DefaultTemperature = 0.2
    DefaultMaxTokens = 0
    DefaultContextWindow = 0
}

# ==============================================================================
# CONFIGURATION FUNCTIONS
# ==============================================================================

function Set-OllamaConfig {
    <#
    .SYNOPSIS
        Configure Ollama client settings.
    .PARAMETER ApiUrl
        Ollama API endpoint URL (default: http://localhost:11434).
    .PARAMETER DefaultTimeout
        Default timeout in seconds for API calls.
    .PARAMETER DefaultTemperature
        Default temperature for text generation (0.0-1.0).
    .PARAMETER DefaultMaxTokens
        Default maximum tokens to generate.
    .PARAMETER DefaultContextWindow
        Default context window size (num_ctx).
    #>
    param(
        [string]$ApiUrl,
        [int]$DefaultTimeout,
        [double]$DefaultTemperature,
        [int]$DefaultMaxTokens,
        [int]$DefaultContextWindow
    )
    
    if ($ApiUrl) {
      $script:OllamaConfig.ApiUrl = $ApiUrl
    }

    if ($PSBoundParameters.ContainsKey('DefaultTimeout')) {
      $script:OllamaConfig.DefaultTimeout = $DefaultTimeout
    }

    if ($PSBoundParameters.ContainsKey('DefaultTemperature')) {
      $script:OllamaConfig.DefaultTemperature = $DefaultTemperature
    }

    if ($PSBoundParameters.ContainsKey('DefaultMaxTokens')) {
      $script:OllamaConfig.DefaultMaxTokens = $DefaultMaxTokens
    }

    if ($PSBoundParameters.ContainsKey('DefaultContextWindow')) {
      $script:OllamaConfig.DefaultContextWindow = $DefaultContextWindow
    }
}

function Get-OllamaConfig {
    <#
    .SYNOPSIS
        Get current Ollama client configuration.
    #>
    return $script:OllamaConfig.Clone()
}

# ==============================================================================
# CONNECTION & STATUS
# ==============================================================================

function Test-OllamaAvailable {
    <#
    .SYNOPSIS
        Check if Ollama API is available and responding.
    .OUTPUTS
        Boolean indicating if Ollama is available.
    #>
    try {
        $null = Invoke-RestMethod -Uri "$($script:OllamaConfig.ApiUrl)/api/tags" -TimeoutSec 5 -ErrorAction Stop
        return $true
    }
    catch {
        return $false
    }
}

function Get-OllamaModels {
    <#
    .SYNOPSIS
        Get list of available models from Ollama.
    .OUTPUTS
        Array of model objects with name, size, and other properties.
    #>
    try {
        $response = Invoke-RestMethod -Uri "$($script:OllamaConfig.ApiUrl)/api/tags" -TimeoutSec 10 -ErrorAction Stop
        return $response.models
    }
    catch {
        Write-Warning "Failed to get Ollama models: $_"
        return @()
    }
}

function Test-OllamaModel {
    <#
    .SYNOPSIS
        Check if a specific model is available in Ollama.
    .PARAMETER Model
        Model name to check.
    #>
    param([Parameter(Mandatory)][string]$Model)
    
    $models = Get-OllamaModels
    return ($models | Where-Object { $_.name -eq $Model -or $_.name -like "${Model}:*" }) -ne $null
}

# ==============================================================================
# TEXT GENERATION
# ==============================================================================

function Invoke-OllamaPrompt {
    <#
    .SYNOPSIS
        Send a prompt to an Ollama model and get a response.
    .PARAMETER Model
        Model name (e.g., "llama3.1:8b", "qwen2.5-coder:7b").
    .PARAMETER Prompt
        The prompt text to send.
    .PARAMETER ContextWindow
        Context window size (num_ctx). Uses default if not specified.
    .PARAMETER MaxTokens
        Maximum tokens to generate (num_predict). Uses default if not specified.
    .PARAMETER Temperature
        Temperature for generation (0.0-1.0). Uses default if not specified.
    .PARAMETER Timeout
        Timeout in seconds. Uses default if not specified.
    .PARAMETER System
        Optional system prompt.
    .OUTPUTS
        Generated text response or $null if failed.
    #>
    param(
        [Parameter(Mandatory)][string]$Model,
        [Parameter(Mandatory)][string]$Prompt,
        [int]$ContextWindow,
        [int]$MaxTokens,
        [double]$Temperature,
        [int]$Timeout,
        [string]$System
    )
    
    $config = $script:OllamaConfig
    
    # Use defaults if not specified
    if (-not $PSBoundParameters.ContainsKey('MaxTokens')) { $MaxTokens = $config.DefaultMaxTokens }
    if (-not $PSBoundParameters.ContainsKey('Temperature')) { $Temperature = $config.DefaultTemperature }
    if (-not $PSBoundParameters.ContainsKey('Timeout')) { $Timeout = $config.DefaultTimeout }
    
    $options = @{
        temperature = $Temperature
    }
    
    # Only set num_predict if MaxTokens > 0 (0 = unlimited/model default)
    if ($MaxTokens -and $MaxTokens -gt 0) {
        $options.num_predict = $MaxTokens
    }
    
    # Only set context window if explicitly provided (let model use its default otherwise)
    if ($ContextWindow -and $ContextWindow -gt 0) {
        $options.num_ctx = $ContextWindow
    }
    
    $body = @{
        model = $Model
        prompt = $Prompt
        stream = $false
        options = $options
    }
    
    if ($System) {
        $body.system = $System
    }
    
    $jsonBody = $body | ConvertTo-Json -Depth 3
    
    # TimeoutSec 0 = infinite wait
    $restParams = @{
        Uri = "$($config.ApiUrl)/api/generate"
        Method = "Post"
        Body = $jsonBody
        ContentType = "application/json"
    }
    if ($Timeout -gt 0) { $restParams.TimeoutSec = $Timeout }
    
    try {
        $response = Invoke-RestMethod @restParams
        return $response.response.Trim()
    }
    catch {
        Write-Warning "Ollama prompt failed: $_"
        return $null
    }
}

function Invoke-OllamaChat {
    <#
    .SYNOPSIS
        Send a chat conversation to an Ollama model.
    .PARAMETER Model
        Model name.
    .PARAMETER Messages
        Array of message objects with 'role' and 'content' properties.
        Roles: "system", "user", "assistant"
    .PARAMETER ContextWindow
        Context window size.
    .PARAMETER MaxTokens
        Maximum tokens to generate.
    .PARAMETER Temperature
        Temperature for generation.
    .OUTPUTS
        Generated response text or $null if failed.
    #>
    param(
        [Parameter(Mandatory)][string]$Model,
        [Parameter(Mandatory)][array]$Messages,
        [int]$ContextWindow,
        [int]$MaxTokens,
        [double]$Temperature,
        [int]$Timeout
    )
    
    $config = $script:OllamaConfig
    
    if (-not $PSBoundParameters.ContainsKey('MaxTokens')) { $MaxTokens = $config.DefaultMaxTokens }
    if (-not $PSBoundParameters.ContainsKey('Temperature')) { $Temperature = $config.DefaultTemperature }
    if (-not $PSBoundParameters.ContainsKey('Timeout')) { $Timeout = $config.DefaultTimeout }
    
    $options = @{
        temperature = $Temperature
    }
    
    # Only set num_predict if MaxTokens > 0 (0 = unlimited/model default)
    if ($MaxTokens -and $MaxTokens -gt 0) {
        $options.num_predict = $MaxTokens
    }
    
    # Only set context window if explicitly provided
    if ($ContextWindow -and $ContextWindow -gt 0) {
        $options.num_ctx = $ContextWindow
    }
    
    $body = @{
        model = $Model
        messages = $Messages
        stream = $false
        options = $options
    }
    
    $jsonBody = $body | ConvertTo-Json -Depth 4
    
    # TimeoutSec 0 = infinite wait
    $restParams = @{
        Uri = "$($config.ApiUrl)/api/chat"
        Method = "Post"
        Body = $jsonBody
        ContentType = "application/json"
    }
    if ($Timeout -gt 0) { $restParams.TimeoutSec = $Timeout }
    
    try {
        $response = Invoke-RestMethod @restParams
        return $response.message.content.Trim()
    }
    catch {
        Write-Warning "Ollama chat failed: $_"
        return $null
    }
}

# ==============================================================================
# EMBEDDINGS
# ==============================================================================

function Get-OllamaEmbedding {
    <#
    .SYNOPSIS
        Get embedding vector for text using an Ollama embedding model.
    .PARAMETER Model
        Embedding model name (e.g., "nomic-embed-text", "mxbai-embed-large").
    .PARAMETER Text
        Text to embed.
    .PARAMETER Timeout
        Timeout in seconds.
    .OUTPUTS
        Array of doubles representing the embedding vector, or $null if failed.
    #>
    param(
        [Parameter(Mandatory)][string]$Model,
        [Parameter(Mandatory)][string]$Text,
        [int]$Timeout = 30
    )
    
    $body = @{
        model = $Model
        prompt = $Text
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$($script:OllamaConfig.ApiUrl)/api/embeddings" -Method Post -Body $body -ContentType "application/json" -TimeoutSec $Timeout
        return $response.embedding
    }
    catch {
        Write-Warning "Ollama embedding failed: $_"
        return $null
    }
}

function Get-OllamaEmbeddings {
    <#
    .SYNOPSIS
        Get embeddings for multiple texts (batch).
    .PARAMETER Model
        Embedding model name.
    .PARAMETER Texts
        Array of texts to embed.
    .PARAMETER ShowProgress
        Show progress indicator.
    .OUTPUTS
        Array of objects with Text and Embedding properties.
    #>
    param(
        [Parameter(Mandatory)][string]$Model,
        [Parameter(Mandatory)][string[]]$Texts,
        [switch]$ShowProgress
    )
    
    $results = @()
    $total = $Texts.Count
    $current = 0
    
    foreach ($text in $Texts) {
        $current++
        if ($ShowProgress) {
            Write-Progress -Activity "Getting embeddings" -Status "$current of $total" -PercentComplete (($current / $total) * 100)
        }
        
        $embedding = Get-OllamaEmbedding -Model $Model -Text $text
        if ($embedding) {
            $results += @{
                Text = $text
                Embedding = $embedding
            }
        }
    }
    
    if ($ShowProgress) {
        Write-Progress -Activity "Getting embeddings" -Completed
    }
    
    return $results
}

# ==============================================================================
# RAG UTILITIES
# ==============================================================================

function Get-CosineSimilarity {
    <#
    .SYNOPSIS
        Calculate cosine similarity between two embedding vectors.
    .PARAMETER Vector1
        First embedding vector.
    .PARAMETER Vector2
        Second embedding vector.
    .OUTPUTS
        Cosine similarity value between -1 and 1.
    #>
    param(
        [Parameter(Mandatory)][double[]]$Vector1, 
        [Parameter(Mandatory)][double[]]$Vector2
    )
    
    if ($Vector1.Length -ne $Vector2.Length) {
        Write-Warning "Vector lengths don't match: $($Vector1.Length) vs $($Vector2.Length)"
        return 0
    }
    
    $dotProduct = 0.0
    $norm1 = 0.0
    $norm2 = 0.0
    
    for ($i = 0; $i -lt $Vector1.Length; $i++) {
        $dotProduct += $Vector1[$i] * $Vector2[$i]
        $norm1 += $Vector1[$i] * $Vector1[$i]
        $norm2 += $Vector2[$i] * $Vector2[$i]
    }
    
    $norm1 = [Math]::Sqrt($norm1)
    $norm2 = [Math]::Sqrt($norm2)
    
    if ($norm1 -eq 0 -or $norm2 -eq 0) { return 0 }
    return $dotProduct / ($norm1 * $norm2)
}

function Group-TextsByEmbedding {
    <#
    .SYNOPSIS
        Cluster texts by embedding similarity.
    .PARAMETER Model
        Embedding model name.
    .PARAMETER Texts
        Array of texts to cluster.
    .PARAMETER SimilarityThreshold
        Minimum cosine similarity to group texts together (0.0-1.0).
    .PARAMETER ShowProgress
        Show progress during embedding.
    .OUTPUTS
        Array of clusters (each cluster is an array of texts).
    #>
    param(
        [Parameter(Mandatory)][string]$Model,
        [Parameter(Mandatory)][string[]]$Texts,
        [double]$SimilarityThreshold = 0.65,
        [switch]$ShowProgress
    )
    
    if ($Texts.Length -eq 0) { return @() }
    if ($Texts.Length -eq 1) { return @(,@($Texts[0])) }
    
    # Get embeddings
    $embeddings = Get-OllamaEmbeddings -Model $Model -Texts $Texts -ShowProgress:$ShowProgress
    
    if ($embeddings.Length -eq 0) { 
        return @($Texts | ForEach-Object { ,@($_) })
    }
    
    # Mark all as unclustered
    $embeddings | ForEach-Object { $_.Clustered = $false }
    
    # Cluster similar texts
    $clusters = @()
    
    for ($i = 0; $i -lt $embeddings.Length; $i++) {
        if ($embeddings[$i].Clustered) { continue }
        
        $cluster = @($embeddings[$i].Text)
        $embeddings[$i].Clustered = $true
        
        for ($j = $i + 1; $j -lt $embeddings.Length; $j++) {
            if ($embeddings[$j].Clustered) { continue }
            
            $similarity = Get-CosineSimilarity -Vector1 $embeddings[$i].Embedding -Vector2 $embeddings[$j].Embedding
            
            if ($similarity -ge $SimilarityThreshold) {
                $cluster += $embeddings[$j].Text
                $embeddings[$j].Clustered = $true
            }
        }
        
        $clusters += ,@($cluster)
    }
    
    return $clusters
}

function Find-SimilarTexts {
    <#
    .SYNOPSIS
        Find texts most similar to a query using embeddings.
    .PARAMETER Model
        Embedding model name.
    .PARAMETER Query
        Query text to find similar texts for.
    .PARAMETER Texts
        Array of texts to search through.
    .PARAMETER TopK
        Number of most similar texts to return.
    .PARAMETER MinSimilarity
        Minimum similarity threshold.
    .OUTPUTS
        Array of objects with Text and Similarity properties, sorted by similarity.
    #>
    param(
        [Parameter(Mandatory)][string]$Model,
        [Parameter(Mandatory)][string]$Query,
        [Parameter(Mandatory)][string[]]$Texts,
        [int]$TopK = 5,
        [double]$MinSimilarity = 0.0
    )
    
    # Get query embedding
    $queryEmbedding = Get-OllamaEmbedding -Model $Model -Text $Query
    if (-not $queryEmbedding) { return @() }
    
    # Get text embeddings and calculate similarities
    $results = @()
    foreach ($text in $Texts) {
        $textEmbedding = Get-OllamaEmbedding -Model $Model -Text $text
        if ($textEmbedding) {
            $similarity = Get-CosineSimilarity -Vector1 $queryEmbedding -Vector2 $textEmbedding
            if ($similarity -ge $MinSimilarity) {
                $results += @{
                    Text = $text
                    Similarity = $similarity
                }
            }
        }
    }
    
    # Sort by similarity and return top K
    return $results | Sort-Object -Property Similarity -Descending | Select-Object -First $TopK
}

# ==============================================================================
# MODULE EXPORTS
# ==============================================================================

Export-ModuleMember -Function @(
    # Configuration
    'Set-OllamaConfig'
    'Get-OllamaConfig'
    
    # Connection & Status
    'Test-OllamaAvailable'
    'Get-OllamaModels'
    'Test-OllamaModel'
    
    # Text Generation
    'Invoke-OllamaPrompt'
    'Invoke-OllamaChat'
    
    # Embeddings
    'Get-OllamaEmbedding'
    'Get-OllamaEmbeddings'
    
    # RAG Utilities
    'Get-CosineSimilarity'
    'Group-TextsByEmbedding'
    'Find-SimilarTexts'
)
