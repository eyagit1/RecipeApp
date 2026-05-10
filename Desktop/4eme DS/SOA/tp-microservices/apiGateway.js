const express = require('express');
const { ApolloServer } = require('@apollo/server');
const { expressMiddleware } = require('@as-integrations/express4');
const cors = require('cors');
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const fs = require('fs');

const resolvers = require('./resolvers');
const typeDefs = fs.readFileSync('./schema.gql', 'utf8');

const movieProto = grpc.loadPackageDefinition(
  protoLoader.loadSync('movie.proto', { keepCase: true, longs: String, enums: String, defaults: true, oneofs: true })
).movie;

const tvShowProto = grpc.loadPackageDefinition(
  protoLoader.loadSync('tvShow.proto', { keepCase: true, longs: String, enums: String, defaults: true, oneofs: true })
).tvShow;

const app = express();
const server = new ApolloServer({ typeDefs, resolvers });

server.start().then(() => {
  app.use('/graphql', cors(), express.json(), expressMiddleware(server));
});

// ── REST: Movies ──────────────────────────────────────────────
app.get('/movies', (req, res) => {
  const client = new movieProto.MovieService('localhost:50051', grpc.credentials.createInsecure());
  client.searchMovies({}, (err, response) => {
    if (err) res.status(500).send(err);
    else res.json(response.movies);
  });
});

app.get('/movies/:id', (req, res) => {
  const client = new movieProto.MovieService('localhost:50051', grpc.credentials.createInsecure());
  client.getMovie({ movie_id: req.params.id }, (err, response) => {
    if (err) res.status(500).send(err);
    else res.json(response.movie);
  });
});

// ── REST: TV Shows ────────────────────────────────────────────
app.get('/tvshows', (req, res) => {
  const client = new tvShowProto.TVShowService('localhost:50052', grpc.credentials.createInsecure());
  client.searchTvshows({}, (err, response) => {
    if (err) res.status(500).send(err);
    else res.json(response.tv_shows);
  });
});

app.get('/tvshows/:id', (req, res) => {
  const client = new tvShowProto.TVShowService('localhost:50052', grpc.credentials.createInsecure());
  client.getTvshow({ tv_show_id: req.params.id }, (err, response) => {
    if (err) res.status(500).send(err);
    else res.json(response.tv_show);
  });
});

app.post('/movies', (req, res) => {
  const client = new movieProto.MovieService('localhost:50051', grpc.credentials.createInsecure());
  client.createMovie(req.body, (err, response) => {
    if (err) res.status(500).send(err);
    else res.status(201).json(response.movie);
  });
});

app.put('/movies/:id', (req, res) => {
  const client = new movieProto.MovieService('localhost:50051', grpc.credentials.createInsecure());
  client.updateMovie({ movie_id: req.params.id, ...req.body }, (err, response) => {
    if (err) res.status(500).send(err);
    else res.json(response.movie);
  });
});

app.delete('/movies/:id', (req, res) => {
  const client = new movieProto.MovieService('localhost:50051', grpc.credentials.createInsecure());
  client.deleteMovie({ movie_id: req.params.id }, (err, response) => {
    if (err) res.status(500).send(err);
    else res.json({ success: response.success });
  });
});

app.listen(3000, () => console.log('API Gateway running on port 3000'));