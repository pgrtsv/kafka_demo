# Введение в Kafka

## Что такое Kafka

Следующий текст - выдержка из https://kafka.apache.org/intro. Рекомендуется прочитать содержимое ссылки самостоятельно.

Apache Kafka - это платформа для потоковой передачи событий. 

Kafka реализует три ключевые функции:

- Публикация (запись) и подписка (чтение) потоков событий, включая непрерывный импорт/экспорт данных из внешних систем.
- Надёжное хранение событий на необходимый срок
- Обработка потоков событий в режиме реального времени либо ретроспективно.

## Необходимый софт

- **Docker+WSL2**, чтобы собирать и запускать Linux-контейнеры.
- (опционально) **.NET 7 SDK**, чтобы собирать клиентские приложения.

## Структура решения

- **KafkaProducer** - простая консольная C# утилита, которая каждые 10 секунд публикует простое сообщение, содержащее дату-время создания, в топик Kafka "time", и логирует это в stdout. 
- **KafkaConsumer** - простая консольная C# утилита, которая подписывается на топик Kafka "time", логирует каждое полученное сообщение в stdout и ждёт одну секунду, имитируя обработку сообщения.
- **Scripts** - простейшие Powershell-скрипты для демонстрации работы с Kafka.
- **compose.yaml** - файл Docker Compose, агрегирующий все необходимые контейнеры.

## Пошаговая демонстрация базовых возможностей

1. Откройте powershell, запустите docker-compose.
    ```powershell
    cd kafka_demo
    docker-compose up
    ```
2. В результате шага 1 должны собраться и запуститься 4 контейнера:
    - kafka-kafka-1 - Kafka
    - kafka-ui-1 - Kafka UI
    - kafka-publisher-1 - KafkaProducer
    - kafka-consumer1-1 - первый KafkaConsumer
    - kafka-consumer2-2 - второй KafkaConsumer

   В Powershell-инстансе, из которого были запущены контейнеры, можно увидеть stdout всех контейнеров, включая паблишера и консюмеров/подписчиков:
   ```
   kafka_demo-publisher-1  | [07:36:24 INF] Message sent.
   kafka_demo-consumer2-1  | [07:36:24 INF] Received message 04/24/2023 07:36:24.
   kafka_demo-publisher-1  | [07:36:34 INF] Message sent.
   kafka_demo-consumer2-1  | [07:36:34 INF] Received message 04/24/2023 07:36:34.
   kafka_demo-publisher-1  | [07:36:44 INF] Message sent.
   kafka_demo-consumer2-1  | [07:36:44 INF] Received message 04/24/2023 07:36:44.
   ```
3. Можно заметить, что несмотря на то, что работают два подписчика, только один получает сообщения. Это происходит из-за того, что в Kafka после старта не были созданы никакие топики. Когда паблишер отправил первое сообщение в топик time, Kafka автоматически создал топик с одной партицией (эту функцию желательно отключать в продакшне). Только один подписчик может получать сообщения из одной партиции (следовательно, если у нас больше подписчиков, чем партиций, часть подписчиков не будет получать сообщения вообще). Так как у нас работают два подписчика, увеличим количество партиций в топике time до двух:
   ```powershell
   # Содержимое SetPartitionsToTwo.ps1:
   docker exec kafka-kafka-1 /opt/bitnami/kafka/bin/kafka-topics.sh `
       --bootstrap-server kafka:9092 `
       --alter --topic time --partitions 2
   ```
   Может понадобиться некоторое время, чтобы потребитель начал работать с новой партицией. В логах начнут появляться сообщения от двух подписчиков:
   ```
   kafka_demo-publisher-1  | [07:44:44 INF] Message sent.
   kafka_demo-consumer2-1  | [07:44:44 INF] Received message 04/24/2023 07:44:44.
   kafka_demo-publisher-1  | [07:44:54 INF] Message sent.
   kafka_demo-consumer1-1  | [07:44:54 INF] Received message 04/24/2023 07:44:54.
   ```
4. Так как Kafka хранит все полученные сообщения в течение заданного времени (для автоматически созданного топика - 1 неделя по умолчанию, но это можно изменить на дни, месяцы, годы и т. д.), существует возможность "переиграть" все полученные сообщения в топике без необходимости паблишерам повторно отправлять эти сообщения. Попытаемся сбросить офсеты для всех партиций в топике time: 
   ```powershell
   # Содержимое ReplayMessages.ps1:
   docker exec kafka_demo-kafka-1 /opt/bitnami/kafka/bin/kafka-consumer-groups.sh `
       --bootstrap-server kafka:9092 `
       --group csharp-consumer --reset-offsets --execute --all-topics --to-earliest
   ```
   При попытке выполнения этой команды получим следующую ошибку:   
   ```
   Error: Assignments can only be reset if the group 'csharp-consumer' is inactive, but the current state is Stable.
   ```
   Это означает, что мы должны сперва остановить всех подписчиков в группе csharp-consumer:
   ```powershell
   # Содержимое StopSubscribers.ps1
   docker stop kafka_demo-consumer1-1
   docker stop kafka_demo-consumer2-1
   ```
   Подождём, пока группа csharp-consumer не будет автоматически переведена в состояние EMPTY:
   ```
   kafka_demo-kafka-1      | [2023-04-24 07:59:02,317] INFO [GroupCoordinator 1]: Group csharp-consumer with generation 4 is now empty
   ```
   Попробуем снова сбросить офсеты (ReplayMessages.ps1) и запустить подписчиков (StartSubscribers.ps1), которые начнут повторно получать все ранее отправленные сообщения.
5. Можно заметить, что один подписчик получает больше сообщений, чем другой: 
   ```
   kafka_demo-consumer2-1  | [08:03:10 INF] Received message 04/24/2023 07:50:14.
   kafka_demo-consumer1-1  | [08:03:11 INF] Received message 04/24/2023 08:02:54.
   kafka_demo-consumer2-1  | [08:03:11 INF] Received message 04/24/2023 07:50:44.
   kafka_demo-consumer1-1  | [08:03:12 INF] Received message 04/24/2023 08:03:04.
   kafka_demo-consumer2-1  | [08:03:12 INF] Received message 04/24/2023 07:50:54.
   kafka_demo-consumer2-1  | [08:03:13 INF] Received message 04/24/2023 07:51:14.
   kafka_demo-consumer2-1  | [08:03:14 INF] Received message 04/24/2023 07:51:34.
   kafka_demo-consumer2-1  | [08:03:15 INF] Received message 04/24/2023 07:51:44.
   kafka_demo-consumer2-1  | [08:03:16 INF] Received message 04/24/2023 07:51:54.
   kafka_demo-consumer2-1  | [08:03:17 INF] Received message 04/24/2023 07:52:24.
   kafka_demo-consumer2-1  | [08:03:18 INF] Received message 04/24/2023 07:52:54.
   ```
   Это объясняется тем фактом, что в рамках одной партиции гарантируется хронологический порядок обработки сообщений. До того, как мы увеличили количество партиций в шаге 3, все получаемые сообщения хранились в одной партиции. Следовательно, консюмер, подписанный на первую партицию, получит больше сообщений, чем консюмер, подписанный на вторую. Попробуем сократить количество партиций в топике time с 2 до 1:
   ```powershell
   # Содержимое SetPartitionsToOne.ps1:
   docker exec kafka_demo-kafka-1 /opt/bitnami/kafka/bin/kafka-topics.sh `
       --bootstrap-server kafka:9092 `
       --alter --topic time --partitions 1
   ```
   Получим ошибку:
   ```
   Error while executing topic command : The topic time currently has 2 partition(s); 1 would not be an increase.
   ```
   Сокращение количества партиций привело бы к удалению партиций и, соответственно, к потере сообщений либо к утрате гарантии очерёдности сообщений в одной партиции. В средах разработки можно удалить и пересоздать весь топик, задав меньшее количество партиций - это приведёт к потере ранее отправленных сообщений (см. DeleteTopic.ps1). В продакшн средах такую операцию не стоит выполнять вообще.

6. Взглянем на исходный код паблишера (KafkaProducer):
   ```csharp
   using var producer = new ProducerBuilder<Null, string>(config).Build();
   ```
   Первый тип-параметр (Null) соответствует типу ключа сообщения, второй (string) - типу тела сообщения. Null означает, что ни одно сообщение, публикуемое этим продюсером в топик time, не будет содержать ключа. Следовательно, публикуемые сообщения будут равномерно распределяться между партициями топика time и между подписчиками. Такое поведение может быть нежелательно, когда необходимо гарантировать порядок обработки сообщений, т. е. гарантировать, что отправленные сообщения будут помещены в одну партицию. Для этого в отправляемых сообщениях задают непустой ключ. Kafka помещает сообщения с одинаковым ключом в одну партицию. 

## Полезные ссылки

- [Apache Kafka](https://kafka.apache.org/intro)
- [Клиент Kafka для C# от Confluent](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
- [Хабр - Учимся жить с Kafka без Zookeeper](https://habr.com/ru/companies/otus/articles/670440/)