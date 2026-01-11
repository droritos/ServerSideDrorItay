const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'https://localhost:7297';

const PROXY_CONFIG = [
  {
    context: [
      "/weatherforecast",
      "/api" // <--- Make sure "/api" is listed here so it finds your GameController!
    ],
    target: "https://localhost:7297", // <--- CHANGE THIS NUMBER TO 7297
    secure: false
  }
];

module.exports = PROXY_CONFIG;
