# Step 1: Create a new .NET solution
dotnet new sln -n rabbitmq-hello-world

# Step 2: Create the 'Send' console application
dotnet new console -n Send
dotnet add Send/Send.csproj package RabbitMQ.Client
dotnet sln rabbitmq-hello-world.sln add Send/Send.csproj
mv Send/Program.cs Send/Send.cs

# Step 3: Create the 'Receive' console application
dotnet new console -n Receive
dotnet add Receive/Receive.csproj package RabbitMQ.Client
dotnet sln rabbitmq-hello-world.sln add Receive/Receive.csproj
mv Receive/Program.cs Receive/Receive.cs