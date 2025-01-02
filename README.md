# CSHARP-etcd-cli
A lightweight and efficient CLI tool built in C# for managing etcd, providing seamless interaction with key-value operations, cluster management, and more. 

## Features
- Connect to etcd clusters
- Perform key-value operations (get, put, delete, list)
- Authentication support

## Installation
1. Clone the repository:
    ```sh
    git clone https://github.com/XSidik/CSHARP-etcd-cli.git
    ```
2. Navigate to the project directory:
    ```sh
    cd CSHARP-etcd-cli/etcdCli
    ```
3. Build the project:
    ```sh
    dotnet build
    ```
4. Start etcd using docker
    ```sh
    docker-compose -f docker-compose-etcd.yml up -d
    ```
5. Set credential etcd
    ```sh
    echo -e "rootpassword123\nrootpassword123" | docker exec -i etcd etcdctl user add root --interactive=false
    ```
6. enable authentication in etcd
    ```sh
        docker exec etcd etcdctl auth enable
    ```

## Usage
1. Run the CLI tool:
    ```sh
    dotnet run
    ```
2. Use the available commands to interact with your etcd cluster.

## Commands
- `get [key]` - Retrieve the value of a key
- `put [key] [value]` - Store a key-value pair
- `delete [key]` - Remove a key
- `list` - Get all list key and value
- `watch [key]` - Monitors changes to keys in etcd and outputs updates in real-time.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.
