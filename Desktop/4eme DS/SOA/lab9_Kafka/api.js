require('dotenv').config();

const express = require('express');
const connectDB = require('./mongo');
const Message = require('./Message');


connectDB();

const app = express();
app.use(express.json());

app.get('/messages', async (req, res) => {
  try {
    const messages = await Message.find().sort({ createdAt: -1 });
    res.json(messages);
  } catch (error) {
    res.status(500).json({
      message: 'Erreur serveur',
      error: error.message,
    });
  }
});

app.get('/messages/:id', async (req, res) => {
  try {
    const message = await Message.findById(req.params.id);

    if (!message) {
      return res.status(404).json({
        message: 'Message introuvable',
      });
    }

    res.json(message);
  } catch (error) {
    res.status(500).json({
      message: 'Erreur serveur',
      error: error.message,
    });
  }
});

const PORT = process.env.PORT || 3000;

app.listen(PORT, () => {
  console.log(`API démarrée sur http://localhost:${PORT}`);
});