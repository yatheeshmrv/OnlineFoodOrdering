
param(
    [string]$ProjectRoot = (Get-Location).Path
)

$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.Drawing

if (-not (Test-Path (Join-Path $ProjectRoot "angular.json"))) {
    throw "Run this script from the food-ordering-ui folder. angular.json was not found in: $ProjectRoot"
}

$foodRoot = Join-Path $ProjectRoot "public\images\food"
$backupRoot = Join-Path $ProjectRoot ("public\images\food-backup-openverse-" + (Get-Date -Format "yyyyMMdd-HHmmss"))

if (Test-Path $foodRoot) {
    Write-Host "Backing up current food images..." -ForegroundColor Cyan
    Copy-Item $foodRoot $backupRoot -Recurse -Force
}

$items = @(
    # PIZZA
    @{ Folder="pizza"; File="bbq-chicken-pizza.jpg"; Queries=@("BBQ chicken pizza","barbecue chicken pizza"); Include=@("pizza","chicken"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="pizza"; File="cheese-burst-pizza.jpg"; Queries=@("cheese pizza","four cheese pizza"); Include=@("pizza","cheese"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="pizza"; File="chicken-pepperoni-pizza.jpg"; Queries=@("pepperoni pizza","chicken pepperoni pizza"); Include=@("pizza","pepperoni"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="pizza"; File="farmhouse-pizza.jpg"; Queries=@("vegetable pizza mushroom pepper","vegetarian pizza"); Include=@("pizza"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="pizza"; File="margherita-pizza.jpg"; Queries=@("margherita pizza"); Include=@("pizza","margherita"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="pizza"; File="paneer-tikka-pizza.jpg"; Queries=@("paneer pizza","paneer tikka pizza"); Include=@("pizza","paneer"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="pizza"; File="veggie-supreme-pizza.jpg"; Queries=@("vegetable pizza olives","veggie pizza"); Include=@("pizza"); Exclude=@("logo","menu","illustration","drawing") },

    # BURGER
    @{ Folder="burger"; File="aloo-tikki-burger.jpg"; Queries=@("aloo tikki burger","potato patty burger"); Include=@("burger"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="burger"; File="classic-chicken-burger.jpg"; Queries=@("chicken burger"); Include=@("burger","chicken"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="burger"; File="classic-veg-burger.jpg"; Queries=@("veggie burger","vegetable burger"); Include=@("burger"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="burger"; File="double-cheese-burger.jpg"; Queries=@("double cheeseburger","cheeseburger"); Include=@("burger","cheese"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="burger"; File="grilled-chicken-burger.jpg"; Queries=@("grilled chicken burger","chicken burger"); Include=@("burger","chicken"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="burger"; File="paneer-crunch-burger.jpg"; Queries=@("paneer burger","Indian vegetarian burger"); Include=@("burger"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="burger"; File="spicy-chicken-burger.jpg"; Queries=@("spicy chicken burger","fried chicken burger"); Include=@("burger","chicken"); Exclude=@("logo","menu","illustration","drawing") },

    # BIRYANI
    @{ Folder="biryani"; File="chicken-biryani.jpg"; Queries=@("chicken biryani"); Include=@("biryani","chicken"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="biryani"; File="egg-biryani.jpg"; Queries=@("egg biryani"); Include=@("biryani","egg"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="biryani"; File="hyderabadi-chicken-biryani.jpg"; Queries=@("Hyderabadi chicken biryani","Hyderabad biryani"); Include=@("biryani"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="biryani"; File="mutton-biryani.jpg"; Queries=@("mutton biryani","lamb biryani"); Include=@("biryani"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="biryani"; File="paneer-biryani.jpg"; Queries=@("paneer biryani","vegetable biryani paneer"); Include=@("biryani"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="biryani"; File="special-family-biryani.jpg"; Queries=@("biryani platter","chicken biryani serving"); Include=@("biryani"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="biryani"; File="veg-biryani.jpg"; Queries=@("vegetable biryani","veg biryani"); Include=@("biryani"); Exclude=@("logo","menu","illustration","drawing") },

    # DRINKS
    @{ Folder="drinks"; File="cold-coffee.jpg"; Queries=@("cold coffee glass","iced coffee"); Include=@("coffee"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="drinks"; File="fresh-lime-soda.jpg"; Queries=@("lime soda drink","lime lemonade"); Include=@("lime"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="drinks"; File="mango-lassi.jpg"; Queries=@("mango lassi"); Include=@("mango","lassi"); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="drinks"; File="masala-buttermilk.jpg"; Queries=@("masala chaas","spiced buttermilk","Indian buttermilk"); Include=@(); Exclude=@("logo","menu","illustration","drawing") },
    @{ Folder="drinks"; File="soft-drink.jpg"; Queries=@("cola glass ice","soft drink glass"); Include=@(); Exclude=@("logo","brand","menu","illustration","drawing") },
    @{ Folder="drinks"; File="sweet-lassi.jpg"; Queries=@("sweet lassi","lassi drink"); Include=@("lassi"); Exclude=@("mango","logo","menu","illustration","drawing") },
    @{ Folder="drinks"; File="watermelon-juice.jpg"; Queries=@("watermelon juice","watermelon smoothie"); Include=@("watermelon"); Exclude=@("logo","menu","illustration","drawing") },

    # DESSERTS
    @{ Folder="desserts"; File="caramel-custard.jpg"; Queries=@("caramel custard","creme caramel"); Include=@("caramel"); Exclude=@("cake","logo","menu","illustration","drawing") },
    @{ Folder="desserts"; File="chocolate-brownie.jpg"; Queries=@("chocolate brownie"); Include=@("brownie"); Exclude=@("cake","logo","menu","illustration","drawing") },
    @{ Folder="desserts"; File="chocolate-ice-cream.jpg"; Queries=@("chocolate ice cream"); Include=@("ice","cream","chocolate"); Exclude=@("cake","logo","menu","illustration","drawing") },
    @{ Folder="desserts"; File="fruit-salad-with-ice-cream.jpg"; Queries=@("fruit salad ice cream","fruit sundae"); Include=@("fruit"); Exclude=@("cake","logo","menu","illustration","drawing") },
    @{ Folder="desserts"; File="gulab-jamun.jpg"; Queries=@("gulab jamun"); Include=@("gulab","jamun"); Exclude=@("cake","logo","menu","illustration","drawing") },
    @{ Folder="desserts"; File="rasmalai.jpg"; Queries=@("rasmalai","ras malai Indian dessert"); Include=@("rasmalai"); Exclude=@("cake","cheesecake","birthday","logo","menu","illustration","drawing") },
    @{ Folder="desserts"; File="vanilla-ice-cream.jpg"; Queries=@("vanilla ice cream"); Include=@("ice","cream"); Exclude=@("chocolate","cake","logo","menu","illustration","drawing") },

    # HEALTHY
    @{ Folder="healthy"; File="brown-rice-veg-bowl.jpg"; Queries=@("brown rice vegetable bowl","brown rice vegetables"); Include=@("rice"); Exclude=@("meat","chicken","fish","logo","menu","illustration","drawing") },
    @{ Folder="healthy"; File="grilled-chicken-salad.jpg"; Queries=@("grilled chicken salad"); Include=@("chicken","salad"); Exclude=@("burger","pizza","logo","menu","illustration","drawing") },
    @{ Folder="healthy"; File="millet-dosa-combo.jpg"; Queries=@("millet dosa","dosa sambar chutney"); Include=@("dosa"); Exclude=@("cake","logo","menu","illustration","drawing") },
    @{ Folder="healthy"; File="oats-vegetable-khichdi.jpg"; Queries=@("oats khichdi","vegetable khichdi"); Include=@("khichdi"); Exclude=@("sweet","cake","logo","menu","illustration","drawing") },
    @{ Folder="healthy"; File="paneer-protein-bowl.jpg"; Queries=@("paneer salad bowl","paneer vegetable bowl"); Include=@("paneer"); Exclude=@("pizza","burger","cake","logo","menu","illustration","drawing") },
    @{ Folder="healthy"; File="quinoa-vegetable-bowl.jpg"; Queries=@("quinoa vegetable bowl","quinoa salad"); Include=@("quinoa"); Exclude=@("meat","chicken","fish","logo","menu","illustration","drawing") },
    @{ Folder="healthy"; File="sprouts-chaat.jpg"; Queries=@("sprouts chaat","sprout salad India","mung bean sprouts salad"); Include=@("sprout"); Exclude=@("soup","cake","logo","menu","illustration","drawing") }
)

$headers = @{
    "User-Agent" = "BiteBasketPortfolioImageTool/2.0"
    "Accept" = "application/json"
}

function Invoke-OpenverseJson {
    param([string]$Url)

    for ($attempt = 1; $attempt -le 6; $attempt++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Headers $headers -UseBasicParsing
            Start-Sleep -Milliseconds 1800
            return ($response.Content | ConvertFrom-Json)
        }
        catch {
            $status = $null
            try { $status = [int]$_.Exception.Response.StatusCode } catch {}

            if ($status -eq 429 -and $attempt -lt 6) {
                $wait = [Math]::Min(60, 5 * [Math]::Pow(2, $attempt - 1))
                Write-Host "  Rate limited. Waiting $wait seconds..." -ForegroundColor DarkYellow
                Start-Sleep -Seconds $wait
                continue
            }

            throw
        }
    }
}

function Invoke-DownloadWithRetry {
    param(
        [string]$Url,
        [string]$OutFile
    )

    for ($attempt = 1; $attempt -le 5; $attempt++) {
        try {
            Invoke-WebRequest -Uri $Url -OutFile $OutFile -Headers @{
                "User-Agent" = "BiteBasketPortfolioImageTool/2.0"
            } -UseBasicParsing
            Start-Sleep -Milliseconds 1800
            return
        }
        catch {
            $status = $null
            try { $status = [int]$_.Exception.Response.StatusCode } catch {}

            if ($status -eq 429 -and $attempt -lt 5) {
                $wait = [Math]::Min(60, 5 * [Math]::Pow(2, $attempt - 1))
                Write-Host "  Download rate limited. Waiting $wait seconds..." -ForegroundColor DarkYellow
                Start-Sleep -Seconds $wait
                continue
            }

            throw
        }
    }
}

function Get-CandidateText {
    param($Result)

    $tagText = ""
    if ($Result.tags) {
        $tagText = (($Result.tags | ForEach-Object { $_.name }) -join " ")
    }

    return (
        "$($Result.title) $tagText $($Result.category) $($Result.provider) $($Result.source)"
    ).ToLowerInvariant()
}

function Get-Score {
    param($Result, $Item)

    $text = Get-CandidateText $Result
    $score = 0

    if ($Result.mature -eq $true) {
        return -100000
    }

    foreach ($bad in $Item.Exclude) {
        if ($text.Contains($bad.ToLowerInvariant())) {
            return -100000
        }
    }

    foreach ($needed in $Item.Include) {
        if ($text.Contains($needed.ToLowerInvariant())) {
            $score += 40
        }
        else {
            $score -= 12
        }
    }

    $w = 0
    $h = 0
    if ($Result.width) { $w = [double]$Result.width }
    if ($Result.height) { $h = [double]$Result.height }

    if ($w -ge 1200 -and $h -ge 800) { $score += 40 }
    elseif ($w -ge 800 -and $h -ge 600) { $score += 25 }
    elseif ($w -ge 500 -and $h -ge 400) { $score += 10 }
    else { $score -= 15 }

    if ($w -gt 0 -and $h -gt 0) {
        $ratio = $w / $h
        if ($ratio -ge 0.7 -and $ratio -le 1.45) { $score += 25 }
        elseif ($ratio -ge 0.55 -and $ratio -le 1.8) { $score += 8 }
    }

    if ($Result.license -in @("cc0","pdm","by","by-sa")) {
        $score += 12
    }

    return $score
}

function Find-BestOpenverseImage {
    param($Item)

    $best = $null
    $bestScore = -1000000
    $bestQuery = $null

    foreach ($query in $Item.Queries) {
        $encoded = [uri]::EscapeDataString($query)
        $url = "https://api.openverse.org/v1/images/?q=$encoded&page_size=20&mature=false"

        try {
            $data = Invoke-OpenverseJson $url
        }
        catch {
            Write-Warning "Search failed for '$query': $($_.Exception.Message)"
            continue
        }

        foreach ($result in @($data.results)) {
            $score = Get-Score $result $Item
            if ($score -gt $bestScore) {
                $best = $result
                $bestScore = $score
                $bestQuery = $query
            }
        }

        if ($bestScore -ge 70) {
            break
        }
    }

    if ($null -eq $best -or $bestScore -lt 10) {
        throw "No acceptable Openverse image found."
    }

    return @{
        Result = $best
        Score = $bestScore
        Query = $bestQuery
    }
}

function Save-FittedJpeg {
    param(
        [string]$SourcePath,
        [string]$DestinationPath
    )

    $source = $null

    try {
        $source = [System.Drawing.Image]::FromFile($SourcePath)
    }
    catch {
        throw "Downloaded file is not a Windows-supported image format."
    }

    try {
        $canvasSize = 1024
        $innerSize = 960

        $bitmap = New-Object System.Drawing.Bitmap($canvasSize, $canvasSize)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)

        try {
            $graphics.Clear([System.Drawing.Color]::FromArgb(248, 246, 242))
            $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
            $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality

            $scale = [Math]::Min($innerSize / $source.Width, $innerSize / $source.Height)
            $newW = [int]($source.Width * $scale)
            $newH = [int]($source.Height * $scale)
            $x = [int](($canvasSize - $newW) / 2)
            $y = [int](($canvasSize - $newH) / 2)

            # Neutral background keeps the full source image visible.
            $graphics.DrawImage($source, $x, $y, $newW, $newH)

            $encoderInfo = [System.Drawing.Imaging.ImageCodecInfo]::GetImageEncoders() |
                Where-Object { $_.MimeType -eq "image/jpeg" } |
                Select-Object -First 1

            $qualityEncoder = [System.Drawing.Imaging.Encoder]::Quality
            $encoderParams = New-Object System.Drawing.Imaging.EncoderParameters(1)
            $encoderParams.Param[0] = New-Object System.Drawing.Imaging.EncoderParameter(
                $qualityEncoder,
                [long]92
            )

            try {
                $bitmap.Save($DestinationPath, $encoderInfo, $encoderParams)
            }
            finally {
                $encoderParams.Dispose()
            }
        }
        finally {
            $graphics.Dispose()
            $bitmap.Dispose()
        }
    }
    finally {
        $source.Dispose()
    }
}

foreach ($folder in @("pizza","burger","biryani","drinks","desserts","healthy")) {
    New-Item -ItemType Directory -Path (Join-Path $foodRoot $folder) -Force | Out-Null
}

$tempRoot = Join-Path $env:TEMP ("BiteBasketOpenverse-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

$attributions = New-Object System.Collections.Generic.List[string]
$failures = New-Object System.Collections.Generic.List[string]

try {
    $i = 0

    foreach ($item in $items) {
        $i++
        Write-Host "[$i/42] $($item.File)" -ForegroundColor Yellow

        try {
            $selection = Find-BestOpenverseImage $item
            $result = $selection.Result

            # Prefer the Openverse full-size proxy; fall back to the source URL,
            # then the Openverse thumbnail if necessary.
            $urls = @(
                "https://api.openverse.org/v1/images/$($result.id)/thumb/?full_size=true&compressed=false",
                $result.url,
                $result.thumbnail
            ) | Where-Object { $_ }

            $downloaded = $false
            $normalized = $false

            foreach ($candidateUrl in $urls) {
                $tempFile = Join-Path $tempRoot ("image-" + $i + ".bin")

                try {
                    Invoke-DownloadWithRetry -Url $candidateUrl -OutFile $tempFile

                    $destination = Join-Path (Join-Path $foodRoot $item.Folder) $item.File
                    Save-FittedJpeg -SourcePath $tempFile -DestinationPath $destination

                    $downloaded = $true
                    $normalized = $true
                    break
                }
                catch {
                    Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
                    continue
                }
            }

            if (-not $downloaded -or -not $normalized) {
                throw "Could not download a usable image file."
            }

            $attributions.Add(
                "$($item.File)`r`n" +
                "  Title: $($result.title)`r`n" +
                "  Creator: $($result.creator)`r`n" +
                "  License: $($result.license) $($result.license_version)`r`n" +
                "  License URL: $($result.license_url)`r`n" +
                "  Source page: $($result.foreign_landing_url)`r`n" +
                "  Openverse ID: $($result.id)`r`n" +
                "  Search query: $($selection.Query)`r`n"
            )

            Write-Host "  OK - score $($selection.Score)" -ForegroundColor Green
        }
        catch {
            $message = "$($item.File): $($_.Exception.Message)"
            $failures.Add($message)
            Write-Warning $message
        }
    }
}
finally {
    if (Test-Path $tempRoot) {
        Remove-Item $tempRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}

$attrPath = Join-Path $foodRoot "ATTRIBUTIONS.txt"
@(
    "BiteBasket food image attributions"
    "Generated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")"
    ""
    "Images were discovered through Openverse, which indexes openly licensed media."
    "Review each source/license URL before publishing commercially."
    ""
    ($attributions -join "`r`n")
) | Set-Content -Path $attrPath -Encoding UTF8

Write-Host ""
Write-Host "------------------------------------------------------------" -ForegroundColor DarkGray
Write-Host "Successful: $($items.Count - $failures.Count) / 42" -ForegroundColor Cyan

if ($failures.Count -gt 0) {
    Write-Host ""
    Write-Host "Items that still need replacement:" -ForegroundColor Red
    $failures | ForEach-Object { Write-Host " - $_" -ForegroundColor Red }
    Write-Host ""
    Write-Host "The successful files were kept. Your original set is also preserved in:" -ForegroundColor Yellow
    Write-Host $backupRoot -ForegroundColor DarkYellow
    exit 1
}

Write-Host ""
Write-Host "All 42 images completed successfully." -ForegroundColor Green
Write-Host "No SQL changes are required." -ForegroundColor Green
Write-Host "Restart Angular with: ng serve" -ForegroundColor Green
