param (
    [string]$FunctionName = "lambda-snap-start",      # Replace with your Lambda function name
    [int]$InvocationCount = 10                       # Number of times to invoke the function
)

# Create an array to store the job objects created by this script
$jobs = @()

Write-Host "Starting parallel invocations of Lambda function: $FunctionName" -ForegroundColor Green

# Loop to invoke the Lambda function in parallel
for ($i = 1; $i -le $InvocationCount; $i++) {
    $job = Start-Job -ScriptBlock {
        param($FunctionName, $InvocationIndex)
        try {
            Write-Host "Job ${InvocationIndex}: Invoking Lambda function..." -ForegroundColor Cyan
            
            # File path to store the response
            $responseFile = "response_${InvocationIndex}.json"
            
            # Invoke the Lambda function and write the response to a file
            aws lambda invoke --function-name $FunctionName --payload '\"hello\"' $responseFile | Out-Null
            
            # Read the response from the file
            $response = Get-Content -Path $responseFile -Raw
            
            # Log the response to the console
            Write-Host "Job ${InvocationIndex}: Lambda Response: $response" -ForegroundColor Green
            
            # Optionally delete the response file after reading
            Remove-Item -Path $responseFile -Force
        } catch {
            Write-Host "Job ${InvocationIndex}: Failed with error: $_" -ForegroundColor Red
        }
    } -ArgumentList $FunctionName, $i

    # Add the job to the list
    $jobs += $job
}

# Wait for all the jobs created by this script to complete
Write-Host "Waiting for all jobs to complete..." -ForegroundColor Yellow
$jobs | Wait-Job

# Clean up completed jobs
$jobs | Remove-Job

Write-Host "All jobs completed and cleaned up." -ForegroundColor Green
