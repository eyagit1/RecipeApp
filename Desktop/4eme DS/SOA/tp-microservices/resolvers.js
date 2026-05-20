// resolvers.js
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const movieProtoPath = 'movie.proto';
const tvShowProtoPath = 'tvShow.proto';
const movieProtoDefinition = protoLoader.loadSync(movieProtoPath, {
keepCase: true,
longs: String,
enums: String,
defaults: true,
oneofs: true,
});
const tvShowProtoDefinition = protoLoader.loadSync(tvShowProtoPath, {
keepCase: true,
longs: String,
enums: String,
defaults: true,
oneofs: true,
});
const movieProto = grpc.loadPackageDefinition(movieProtoDefinition).movie;
const tvShowProto = grpc.loadPackageDefinition(tvShowProtoDefinition).tvShow;
const resolvers = {
Query: {
movie: (_, { id }) => {
const client = new movieProto.MovieService('localhost:50051',
grpc.credentials.createInsecure());
return new Promise((resolve, reject) => {
client.getMovie({ movie_id: id }, (err, response) => {
if (err) {
reject(err);
} else {
resolve(response.movie);
}
});
});
},
movies: () => {
const client = new movieProto.MovieService('localhost:50051',
grpc.credentials.createInsecure());
return new Promise((resolve, reject) => {
client.searchMovies({}, (err, response) => {
if (err) {
reject(err);
} else {
resolve(response.movies);
}
});
});
},
tvShow: (_, { id }) => {
const client = new tvShowProto.TVShowService('localhost:50052',
grpc.credentials.createInsecure());
return new Promise((resolve, reject) => {
client.getTvshow({ tv_show_id: id }, (err, response) => {
if (err) {
reject(err);
} else {
resolve(response.tv_show);
}
});
});
},
tvShows: () => {
const client = new tvShowProto.TVShowService('localhost:50052',
grpc.credentials.createInsecure());
return new Promise((resolve, reject) => {
client.searchTvshows({}, (err, response) => {
if (err) {
reject(err);
} else {
resolve(response.tv_shows);
}
});
});
},
},
Mutation: {
    createMovie: (_, { title, description }) =>
      new Promise((resolve, reject) =>
        movieClient().createMovie({ title, description }, (err, res) => err ? reject(err) : resolve(res.movie))
      ),

    updateMovie: (_, { id, title, description }) =>
      new Promise((resolve, reject) =>
        movieClient().updateMovie({ movie_id: id, title: title || '', description: description || '' }, (err, res) => err ? reject(err) : resolve(res.movie))
      ),

    deleteMovie: (_, { id }) =>
      new Promise((resolve, reject) =>
        movieClient().deleteMovie({ movie_id: id }, (err, res) => err ? reject(err) : resolve(res.success))
      ),

    createTVShow: (_, { title, description }) =>
      new Promise((resolve, reject) =>
        tvShowClient().createTvshow({ title, description }, (err, res) => err ? reject(err) : resolve(res.tv_show))
      ),

    updateTVShow: (_, { id, title, description }) =>
      new Promise((resolve, reject) =>
        tvShowClient().updateTvshow({ tv_show_id: id, title: title || '', description: description || '' }, (err, res) => err ? reject(err) : resolve(res.tv_show))
      ),

    deleteTVShow: (_, { id }) =>
      new Promise((resolve, reject) =>
        tvShowClient().deleteTvshow({ tv_show_id: id }, (err, res) => err ? reject(err) : resolve(res.success))
      ),
  },
};

module.exports = resolvers;