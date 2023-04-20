docker exec kafka-kafka-1 /opt/bitnami/kafka/bin/kafka-topics.sh `
    --bootstrap-server kafka:9092 `
    --delete --topic time