docker exec kafka-kafka-1 /opt/bitnami/kafka/bin/kafka-topics.sh `
    --bootstrap-server kafka:9092 `
    --alter --topic time --partitions 2