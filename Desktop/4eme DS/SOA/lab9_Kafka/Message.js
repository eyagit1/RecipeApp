const mongoose = require('mongoose');

const messageSchema = new mongoose.Schema({
  topic: String,
  partition: Number,
  offset: String,
  key: String,
  payload: Object,
  createdAt: {
    type: Date,
    default: Date.now
  }
});
module.exports = mongoose.model('Message', messageSchema);