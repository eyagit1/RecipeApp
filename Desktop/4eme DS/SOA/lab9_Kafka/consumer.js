require('dotenv').config();
const { Kafka } = require('kafkajs');
const connectDB = require('./mongo');
const Message = require('./Message');

connectDB();

const kafka = new Kafka({
  clientId: 'tp9-consumer',
  brokers: [process.env.KAFKA_BROKER],
});

const consumer = kafka.consumer({
  groupId: 'test-group',
});

const run = async () => {
  await consumer.connect();
  console.log('Consumer connecté à Kafka');

  await consumer.subscribe({
    topic: process.env.KAFKA_TOPIC,
    fromBeginning: true,
  });

  await consumer.run({
    eachMessage: async ({ topic, partition, message }) => {
      const key = message.key ? message.key.toString() : null;
      const value = message.value ? message.value.toString() : null;

      let payload;

      try {
        payload = JSON.parse(value);
      } catch (error) {
        console.error('Erreur JSON :', error.message);
        return;
      }

      await Message.create({
        topic,
        partition,
        offset: message.offset,
        key,
        payload,
      });

      console.log('Message enregistré dans MongoDB :', payload);
    },
  });
};

run().catch(console.error);