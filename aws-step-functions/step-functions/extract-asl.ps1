# Run CDK synth if cdk.out doesn't exist
if (-not (Test-Path "./cdk.out")) {
    Write-Host "Running cdk synth..."
    cdk synth
}

# Create ASL Generated folder if it doesn't exist
$outputFolder = "./ASL Generated"
if (-not (Test-Path $outputFolder)) {
    Write-Host "Creating ASL Generated folder..."
    New-Item -ItemType Directory -Path $outputFolder | Out-Null
}

# Find all CloudFormation templates
$templates = Get-ChildItem ./cdk.out -Filter "*.template.json"

foreach ($templateFile in $templates) {
    Write-Host "Processing $($templateFile.Name)..."
    
    $template = Get-Content $templateFile.FullName | ConvertFrom-Json
    
    # Find all state machines in the template
    $stateMachines = $template.Resources.PSObject.Properties | Where-Object { 
        $_.Value.Type -eq 'AWS::StepFunctions::StateMachine' 
    }
    
    foreach ($sm in $stateMachines) {
        $smName = $sm.Name
        Write-Host "Found state machine: $smName"

        # Handle both plain string and Fn::Join formats
        $defString = $null
        $defProp = $sm.Value.Properties.DefinitionString

        if ($defProp -is [string]) {
            # Plain string definition
            $defString = $defProp
        }
        elseif ($defProp.'Fn::Join') {
            # Fn::Join definition
            $defParts = $defProp.'Fn::Join'[1]
            $defString = ($defParts | Where-Object { $_ -is [string] }) -join ''
        }
        else {
            Write-Host "Warning: Unknown definition format for $smName" -ForegroundColor Yellow
            continue
        }

        $outputFile = Join-Path $outputFolder "state-machine-$smName.asl.json"
        $defString | ConvertFrom-Json | ConvertTo-Json -Depth 10 | Out-File $outputFile

        Write-Host "Extracted to $outputFile"
    }
}