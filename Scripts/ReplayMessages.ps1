docker exec kafka_demo-kafka-1 /opt/bitnami/kafka/bin/kafka-consumer-groups.sh `
    --bootstrap-server kafka:9092 `
    --group csharp-consumer --reset-offsets --execute --all-topics --to-earliest