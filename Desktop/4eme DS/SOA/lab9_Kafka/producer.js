require('dotenv').config();

const { Kafka } = require('kafkajs');

const kafka = new Kafka({
  clientId: 'tp9-producer',
  brokers: [process.env.KAFKA_BROKER],
});

const producer = kafka.producer();

const run = async () => {
  await producer.connect();
  console.log('Producer connecté à Kafka');

  setInterval(async () => {
    const event = {
      deviceId: 'sensor-01',
      temperature: Number((20 + Math.random() * 10).toFixed(2)),
      createdAt: new Date().toISOString(),
    };

    await producer.send({
      topic: process.env.KAFKA_TOPIC,
      messages: [
        {
          key: event.deviceId,
          value: JSON.stringify(event),
        },
      ],
    });

    console.log('Message produit :', event);
  }, 1000);
};

run().catch(console.error);