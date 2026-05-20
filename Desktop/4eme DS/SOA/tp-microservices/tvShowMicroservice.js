// tvShowMicroservice.js
const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const tvShowProtoPath = 'tvShow.proto';
const tvShowProtoDefinition = protoLoader.loadSync(tvShowProtoPath, {
keepCase: true,
longs: String,
enums: String,
defaults: true,
oneofs: true,
});
const tvShowProto = grpc.loadPackageDefinition(tvShowProtoDefinition).tvShow;
const tvShowService = {
getTvshow: (call, callback) => {
const tv_show = {
id: call.request.tv_show_id,
title: 'Example of a TV series',
description: 'This is an example of a TV series.',
};
callback(null, { tv_show });
},
searchTvshows: (call, callback) => {
const { query } = call.request;
const tv_shows = [
{
id: '1',
title: 'Example TV Series 1',
description: 'This is the first example of a TV series.',
},
{
id: '2',
title: 'Example TV series 2',
description: 'This is the second example of a TV series.',
},
];
callback(null, { tv_shows });
},
};
const server = new grpc.Server();
server.addService(tvShowProto.TVShowService.service, tvShowService);
const port = 50052;
server.bindAsync(`0.0.0.0:${port}`, grpc.ServerCredentials.createInsecure(),
(err, port) => {
if (err) {
console.error('Server Link Failed:', err);
return;
}
console.log(`The server is running on the port ${port}`);
});
console.log(`TV show microservice running on the port ${port}`);