## Check tools

Check what dotnet tools are installed on the system
`dotnet tool list -g`

## Install dotnet -trace\
Dotnet trace connects to the diagnostic ports. It can save them as dotnet format

`dotnet tool install -g dotnet-trace`

## Check available commands

`dotnet-trace`

## Collect tracing information

`dotnet-trace collect`
`dotnet-trace collect -- dotnet run --project Tracing.csproj `

## Check all the processes that you can collect traces from 

`dotnet-trace ps`

## Collect in Speedscope format
`dotnet-trace collect --format Speedscope  --process-id 73836`