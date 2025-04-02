<#
.SYNOPSIS
Concatenates the content of source code files from a specified directory (like a VS project root)
into a single text file, preserving directory structure information. Includes enhanced debugging output.

.DESCRIPTION
This script recursively searches a given directory for files matching specified extensions.
It excludes specified directories (like bin, obj, .vs, .git).
The content of each found file is appended to an output file, preceded by a header
indicating the file's relative path. Includes extra Write-Host statements for debugging.

.PARAMETER ProjectDirectory
The root directory of the Visual Studio project or solution. The script will search recursively from here.

.PARAMETER OutputFile
The path and name for the resulting large text file. Defaults to 'ConcatenatedSourceCode.txt'
in the ProjectDirectory.

.PARAMETER IncludeExtensions
An array of file extensions (including the dot, e.g., '.cs', '.h') to include.
Defaults to common source code and configuration file types found in VS projects.

.PARAMETER ExcludeDirectories
An array of directory names to exclude from the search (case-insensitive).
Defaults to common VS/Git generated/temporary directories.

.EXAMPLE
.\Concat-SourceCode.ps1 -ProjectDirectory "C:\Path\To\Your\Solution\YourProject"

.EXAMPLE
.\Concat-SourceCode.ps1 -ProjectDirectory "C:\Path\To\Your\Project" -OutputFile "C:\Temp\ProjectOutput.txt" -IncludeExtensions @('.cs', '.xaml', '.config') -ExcludeDirectories @('bin', 'obj', 'Properties')

.NOTES
Author: Your Name / AI Assistant
Date:   2023-10-27 (Modified for Debugging 2023-10-27)
Requires PowerShell 5.1 or later.
Be mindful of file encodings. This script attempts to read/write using UTF-8.
Large projects can result in very large output files.
Enhanced debugging added to trace execution flow.
#>
param(
    [Parameter(Mandatory=$true, HelpMessage="Enter the root path of the Visual Studio project/solution directory.")]
    [string]$ProjectDirectory,

    [Parameter(HelpMessage="Specify the path for the output concatenated text file.")]
    [string]$OutputFile = "",

    [Parameter(HelpMessage="Array of file extensions to include (e.g., '.cs', '.vb').")]
    [string[]]$IncludeExtensions = @(
        '.cs', '.vb', '.fs',        # .NET Languages
        '.cpp', '.c', '.hpp', '.h', # C/C++
        '.js', '.ts', '.jsx', '.tsx', # JavaScript/TypeScript
        '.html', '.htm', '.css', '.scss', '.less', # Web Frontend
        '.xaml', '.razor', '.cshtml', # UI Markup
        '.sql',                    # SQL Scripts
        '.py',                     # Python
        '.sh', '.ps1',             # Shell Scripts
        '.xml', '.json', '.yaml', '.yml', # Configuration/Data
        '.config', '.settings', '.props', '.targets' # Build/Config specific
        # Add or remove extensions as needed
    ),

    [Parameter(HelpMessage="Array of directory names to exclude (case-insensitive).")]
    [string[]]$ExcludeDirectories = @(
        'bin',
        'obj',
        '.vs',
        '.git',
        '.svn',
        'node_modules',
        'packages',
        'TestResults',
        'GeneratedFiles' # Add more if needed
    )
)

# --- Script Start ---

# Validate Project Directory Input
Write-Host "DEBUG: Validating input ProjectDirectory: '$ProjectDirectory'" -ForegroundColor Cyan
if ([string]::IsNullOrWhiteSpace($ProjectDirectory)) {
    Write-Error "ProjectDirectory parameter cannot be empty."
    return
}
if (-not (Test-Path -Path $ProjectDirectory -PathType Container)) {
    Write-Error "Project directory not found or is not a directory: '$ProjectDirectory'"
    return
}

# Resolve the path to a full, standardized string path and store it
$ResolvedProjectDirectoryPath = "" # Initialize
try {
    # Use -LiteralPath to avoid issues with brackets [] in path names
    # Access .ProviderPath to reliably get the string path
    $ResolvedProjectDirectoryPath = (Resolve-Path -LiteralPath $ProjectDirectory).ProviderPath
     Write-Host "DEBUG: Resolved ProjectDirectory path to: '$ResolvedProjectDirectoryPath'" -ForegroundColor Cyan
     if ([string]::IsNullOrWhiteSpace($ResolvedProjectDirectoryPath)) {
          Write-Error "Resolve-Path returned an empty path for '$ProjectDirectory'. Check permissions or path validity."
          return
     }
} catch {
     Write-Error "Failed to resolve project directory path '$ProjectDirectory': $($_.Exception.Message)"
     return
}


# Determine Output File Path using the resolved path
if ([string]::IsNullOrWhiteSpace($OutputFile)) {
    # Use the RESOLVED path string here
    $OutputFilePath = Join-Path -Path $ResolvedProjectDirectoryPath -ChildPath "ConcatenatedSourceCode.txt"
} else {
    # If output path is relative, make it relative to the *current* location (PWD),
    # otherwise use the absolute path provided.
    if ([System.IO.Path]::IsPathRooted($OutputFile)) {
         $OutputFilePath = $OutputFile # Already absolute
    } else {
         # Resolve relative to PWD (current directory)
         try {
            $OutputFilePath = (Resolve-Path -LiteralPath (Join-Path -Path (Get-Location).Path -ChildPath $OutputFile)).ProviderPath
         } catch {
             Write-Error "Failed to resolve relative output path '$OutputFile': $($_.Exception.Message)"
             return
         }
    }
    # Ensure the output directory exists
    $OutputDirectory = Split-Path -Path $OutputFilePath -Parent
    # Check if $OutputDirectory is not null/empty (happens if $OutputFilePath is just a filename in the root)
    if ($OutputDirectory -and (-not (Test-Path -Path $OutputDirectory -PathType Container))) {
        Write-Host "Creating output directory: $OutputDirectory"
        try {
             New-Item -Path $OutputDirectory -ItemType Directory -Force -ErrorAction Stop | Out-Null
        } catch {
             Write-Error "Failed to create output directory '$OutputDirectory': $($_.Exception.Message)"
             return
        }
    }
}
Write-Host "DEBUG: Determined OutputFilePath: '$OutputFilePath'" -ForegroundColor Cyan


# Prepare exclusion pattern for directories (more robust check)
$excludeDirPatterns = $ExcludeDirectories | ForEach-Object { [regex]::Escape($_) }
# Match directory name surrounded by path separators or at the start/end of the relative path
$excludeRegex = '(?i)(^|[\\/])(' + ($excludeDirPatterns -join '|') + ')([\\/]|$)'

Write-Host "Starting source code concatenation..."
# Use the RESOLVED path string for display
Write-Host "Project Directory : $ResolvedProjectDirectoryPath"
Write-Host "Output File       : $OutputFilePath"
Write-Host "Including Exts    : $($IncludeExtensions -join ', ')"
Write-Host "Excluding Dirs    : $($ExcludeDirectories -join ', ')"
Write-Host "Exclusion Regex   : $excludeRegex" # DEBUG: Show the regex

# Clear or create the output file, ensure UTF8 encoding without BOM initially is okay
try {
    # Use the determined $OutputFilePath string
    if ([string]::IsNullOrWhiteSpace($OutputFilePath)) {
         Write-Error "Output file path is unexpectedly empty before writing."
         return
    }
    Set-Content -Path $OutputFilePath -Value "Source Code Concatenation Report" -Encoding UTF8 -ErrorAction Stop
    Add-Content -Path $OutputFilePath -Value "Project Root: $ResolvedProjectDirectoryPath" -Encoding UTF8 # Use RESOLVED path string
    Add-Content -Path $OutputFilePath -Value "Generated On: $(Get-Date)" -Encoding UTF8
    Add-Content -Path $OutputFilePath -Value "Included Extensions: $($IncludeExtensions -join ', ')" -Encoding UTF8
    Add-Content -Path $OutputFilePath -Value "Excluded Directories: $($ExcludeDirectories -join ', ')" -Encoding UTF8
    Add-Content -Path $OutputFilePath -Value "`n" -Encoding UTF8
} catch {
     Write-Error "Failed to initialize output file '$OutputFilePath': $($_.Exception.Message)"
     Return # Cannot proceed if output file cannot be written
}


# --- Start Enhanced Debug Section ---

# Find all files recursively - USE RESOLVED PATH STRING
Write-Host "DEBUG: Starting file search for '$ResolvedProjectDirectoryPath'..." -ForegroundColor Magenta
$OriginalErrorAction = $ErrorActionPreference
$ErrorActionPreference = "Stop" # Make errors terminate the script for easier debugging
$allFiles = $null # Initialize to null
$fileCount = 0    # Initialize count

try {
    # Use the RESOLVED path string here
    $allFiles = Get-ChildItem -Path $ResolvedProjectDirectoryPath -Recurse -File
    Write-Host "DEBUG: Get-ChildItem completed." -ForegroundColor Magenta

    if ($null -eq $allFiles) {
        Write-Host "DEBUG: `$allFiles is `$null after Get-ChildItem (No files found or error occurred)." -ForegroundColor Red
        $fileCount = 0
    } else {
        Write-Host "DEBUG: `$allFiles object type is: $($allFiles.GetType().FullName)" -ForegroundColor Cyan
        # Attempt to get count safely
        if ($allFiles -is [array]) {
             $fileCount = $allFiles.Length # Use .Length for arrays
             Write-Host "DEBUG: Collection is an Array." -ForegroundColor Cyan
        } elseif ($allFiles -is [object]) {
             # If single object, count is 1 (GCI sometimes returns single object if only one found)
             $fileCount = 1
             Write-Host "DEBUG: Collection is a single Object." -ForegroundColor Cyan
        } else {
             Write-Host "DEBUG: Collection is of unexpected type or empty." -ForegroundColor Yellow
             $fileCount = 0
        }
         Write-Host "DEBUG: Calculated file count: $fileCount" -ForegroundColor Cyan

         if ($fileCount -eq 0 -and $allFiles -ne $null) {
             # This case might occur if GCI returns an empty collection explicitly (e.g., an empty array)
              Write-Host "DEBUG: Get-ChildItem returned an empty collection." -ForegroundColor Yellow
         }
    }

} catch {
    Write-Error "FATAL ERROR during Get-ChildItem: $($_.Exception.Message)"
    Return # Exit the script if GCI failed
} finally {
     $ErrorActionPreference = $OriginalErrorAction # Reset error preference
}


$processedCount = 0
# Use the calculated count.
$totalFiles = $fileCount
$includedCount = 0

Write-Host "Found $totalFiles total items based on calculation." # Display the calculated count
Write-Host "DEBUG: Variable `$totalFiles` set to: $totalFiles" -ForegroundColor Magenta
Write-Host "DEBUG: Entering filtering/processing loop..." -ForegroundColor Magenta

# --- End Enhanced Debug Section ---


# Filter and process files
Write-Host "DEBUG: === BEFORE FOREACH LOOP === (Collection type: $($allFiles.GetType().FullName), Count: $totalFiles)" -ForegroundColor Green

# Check if $allFiles is actually iterable
if ($null -ne $allFiles -and $totalFiles -gt 0) {
    # If GCI returned a single object, PowerShell's foreach handles it correctly.
    # If it returned an array/collection, foreach iterates through it.
    foreach ($file in $allFiles) {
        Write-Host "DEBUG: === INSIDE FOREACH LOOP (Start Iteration) ===" -ForegroundColor Cyan # FIRST line inside loop

        $processedCount++
        # Don't let Write-Progress errors stop the script here
        try {
            Write-Progress -Activity "Processing Files" -Status "File $processedCount/${totalFiles}: $($file.Name)" -PercentComplete (($processedCount / $totalFiles) * 100) -ErrorAction SilentlyContinue
        } catch { Write-Warning "Non-fatal error during Write-Progress: $($_.Exception.Message)" }


        # Check extension
        Write-Host "  [Loop $processedCount] Checking Ext: '$($file.Extension)' for '$($file.Name)'" -ForegroundColor Gray
        if ($IncludeExtensions -notcontains $file.Extension) {
            Write-Host "    -> Skipping (Extension)" -ForegroundColor Yellow
            continue
        }

        # Get relative path using the RESOLVED path string as the base
        $relativePath = $null
        try {
            Write-Host "  [Loop $processedCount] Calculating Rel Path for: $($file.FullName)" -ForegroundColor Gray
            if ($PSVersionTable.PSVersion.Major -ge 6) {
                # Use LiteralPath for safety with special characters - USE RESOLVED PATH STRING as base
                $relativePath = (Resolve-Path -LiteralPath $file.FullName -Relative -RelativeTo $ResolvedProjectDirectoryPath).TrimStart(".$([System.IO.Path]::DirectorySeparatorChar)")
            } else {
                 # Ensure ResolvedProjectDirectoryPath ends with a separator for accurate Substring calculation
                 $rootPathWithSep = $ResolvedProjectDirectoryPath # Use RESOLVED PATH STRING
                 if (-not $rootPathWithSep.EndsWith([System.IO.Path]::DirectorySeparatorChar)) {
                    $rootPathWithSep += [System.IO.Path]::DirectorySeparatorChar
                 }

                 if ($file.FullName.StartsWith($rootPathWithSep, [System.StringComparison]::OrdinalIgnoreCase)) {
                     # Calculate substring relative to the resolved root path string
                     $relativePath = $file.FullName.Substring($rootPathWithSep.Length)
                 } else {
                     $relativePath = $file.Name # Fallback
                     Write-Warning "[Loop $processedCount] File '$($file.FullName)' seems outside project root '$ResolvedProjectDirectoryPath'. Using filename."
                 }
            }
             Write-Host "    Rel Path: '$relativePath'" -ForegroundColor Gray
        } catch {
            Write-Warning "[Loop $processedCount] Error calculating relative path for '$($file.FullName)': $($_.Exception.Message)"
            Write-Host "    -> Skipping (Rel Path Error)" -ForegroundColor Red
            continue # Skip if we can't get a relative path reliably
        }

        # Check if the file is within an excluded directory using the calculated relative path
        Write-Host "  [Loop $processedCount] Checking Exclusions for: '$relativePath'" -ForegroundColor Gray
        if ($relativePath -match $excludeRegex) {
            Write-Host "    -> Skipping (Path matched exclusion pattern '$excludeRegex')" -ForegroundColor Yellow
            continue # Skip file if its path contains an excluded directory part
        }

        # --- Process the file ---
        $includedCount++
        Write-Host "  [Loop $processedCount] --> ADDING File: $relativePath" -ForegroundColor Green
        Write-Verbose "Adding: $relativePath" # Keep the verbose one too

        # Create header for the file
        $header = @"

===============================================================================
File: $relativePath
===============================================================================

"@
        # Read content - attempt multiple encodings if needed
        $content = $null
        try {
            # Try UTF8 first
            $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8 -ErrorAction Stop
        } catch [System.IO.IOException] {
             Write-Warning "[Loop $processedCount] Could not read file '$($file.FullName)' (IO Error): $($_.Exception.Message)"
             $content = "*** ERROR READING FILE (IO): $($_.Exception.Message) ***"
        } catch {
             Write-Warning "[Loop $processedCount] Could not read file '$($file.FullName)' (UTF8 decode error likely): $($_.Exception.Message). Trying Default encoding."
             try {
                 $content = Get-Content -Path $file.FullName -Raw -Encoding Default -ErrorAction Stop
             } catch {
                Write-Warning "[Loop $processedCount] Could not read file '$($file.FullName)' with Default encoding either: $($_.Exception.Message)"
                $content = "*** ERROR READING FILE (Both UTF8 and Default failed): $($_.Exception.Message) ***"
             }
        }

        # Append header and content to output file using UTF8
        try {
             Add-Content -Path $OutputFilePath -Value ($header + $content) -Encoding UTF8 -ErrorAction Stop
        } catch {
             Write-Warning "[Loop $processedCount] Error writing to output file '$OutputFilePath': $($_.Exception.Message)"
             # Depending on severity, you might want to 'return' or 'break' here
        }

        Write-Host "DEBUG: === INSIDE FOREACH LOOP (End Iteration $processedCount) ===" -ForegroundColor Cyan
    } # End foreach file loop

} else {
     Write-Host "DEBUG: Skipping foreach loop because `$allFiles is null or empty ($totalFiles files)." -ForegroundColor Yellow
}


Write-Host "DEBUG: === AFTER FOREACH LOOP ===" -ForegroundColor Green
# Ensure Write-Progress completes cleanly
try {
    Write-Progress -Activity "Processing Files" -Completed -ErrorAction SilentlyContinue
} catch { }


Write-Host "--------------------------------------------------"
Write-Host "Concatenation Complete."
Write-Host "Attempted to process $totalFiles total items found." # Use the calculated total
Write-Host "Included $includedCount files in the output."
Write-Host "Output written to: $OutputFilePath"
Write-Host "--------------------------------------------------"

# --- Script End ---