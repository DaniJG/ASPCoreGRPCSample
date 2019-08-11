# NOTE: Dockerfiles generated for ASP.NET Core projects from Visual Studio
#       assume the solution folder as the root folder!
#       This file honours that convention for consistency purposes

FROM node:11.10-slim as dev

WORKDIR /home/node/app
COPY Products/package.json Products/package-lock.json ./Products/
RUN cd ./Products && npm install && cd ..

FROM dev as prod

COPY . .
WORKDIR /home/node/app/Products
EXPOSE 5004
CMD ["npm", "start"]