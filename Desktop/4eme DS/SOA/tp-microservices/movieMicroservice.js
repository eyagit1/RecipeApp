// movieMicroservice.js
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const movieProtoPath = 'movie.proto';
const movieProtoDefinition = protoLoader.loadSync(movieProtoPath, {
keepCase: true,
longs: String,
enums: String,
defaults: true,
oneofs: true,
});
const movieProto = grpc.loadPackageDefinition(movieProtoDefinition).movie;
    const movies = [
  { id: '1', title: 'Example Movie 1', description: 'First example movie.' },
  { id: '2', title: 'Example Movie 2', description: 'Second example movie.' },
];
let nextId = 3;
const movieService = {


// Update searchMovies to use the store:
searchMovies: (call, callback) => {
  callback(null, { movies });
},

getMovie: (call, callback) => {
  const movie = movies.find(m => m.id === call.request.movie_id);
  callback(null, { movie: movie || {} });
},

// Add the new handlers:
createMovie: (call, callback) => {
  const movie = { id: String(nextId++), title: call.request.title, description: call.request.description };
  movies.push(movie);
  callback(null, { movie });
},

updateMovie: (call, callback) => {
  const movie = movies.find(m => m.id === call.request.movie_id);
  if (movie) {
    movie.title = call.request.title || movie.title;
    movie.description = call.request.description || movie.description;
  }
  callback(null, { movie: movie || {} });
},

deleteMovie: (call, callback) => {
  const index = movies.findIndex(m => m.id === call.request.movie_id);
  if (index !== -1) movies.splice(index, 1);
  callback(null, { success: index !== -1 });
},
    
}
const server = new grpc.Server();
server.addService(movieProto.MovieService.service, movieService);
const port = 50051;
server.bindAsync(`0.0.0.0:${port}`, grpc.ServerCredentials.createInsecure(),
(err, port) => {
if (err) {
console.error('Server Link Failed:', err);
return;
}
console.log(`The server is running on the port ${port}`);
});
console.log(`Microservice of movies running on the port ${port}`);