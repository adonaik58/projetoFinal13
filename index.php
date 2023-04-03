<?php
include "models/routing/index.php";

$path = (string)"";
$routes = [];

Route("/", function () {
   global $path;
   $path =  "./view/index.php";
});
Route("/home", function () {
   global $path;
   $path =  "./view/index.php";
});
Route("/dashboard", function () {
   global $path;
   $path =  "./view/index.php";
});

Route("/login", function () {
   global $path;
   $path =  "./view/login.php";
});

Route("/service", function () {
   global $path;
   $path = "./view/servicos.php";
});
Route("/tickets", function () {
   global $path;
   $path = "./view/tickets.php";
});
Route("/spaces", function () {
   global $path;
   $path = "./view/spaces.php";
});

/* Rotas para os operarios */
Route("/gestor/users", function () {
   global $path;
   $path = "./view/operer/users.php";
});
Route("/gestor/tickts-story", function () {
   global $path;
   $path = "./view/operer/story.php";
});
Route("/gestor/parkings", function () {
   global $path;
   $path = "./view/operer/parking.php";
});
Route("/gestor/cars", function () {
   global $path;
   $path = "./view/operer/cars.php";
});
Route("/gestor/marcs", function () {
   global $path;
   $path = "./view/operer/marcs.php";
});
Route("/gestor/cars-models", function () {
   global $path;
   $path = "./view/operer/cars-models.php";
});
Route("/gestor/table-prices", function () {
   global $path;
   $path = "./view/operer/table-prices.php";
});
Route("/gestor/promotion", function () {
   global $path;
   $path = "./view/operer/promotion.php";
});



Route("/404", function () {
   global $path;
   $path = "./view/_404.php";
});



/* API REQUEST FROM FRONT-END APLICATION */

// Login
Route("/api/login/request", function () {
   global $path;
   $path = "./controllers/APIs/user/login.php";
});


// User API request
Route("/api/user/data", function () {
   global $path;
   $path = "./controllers/APIs/user/getData.php";
});

Route("/api/user/new", function () {
   global $path;
   $path = "./controllers/APIs/user/new.php";
});

Route("/api/user/update", function () {
   global $path;
   $path = "./controllers/APIs/user/update.php";
});

// Space API request

Route("/api/space/car/new", function () {
   global $path;
   $path = "./controllers/APIs/space/car/new.php";
});

Route("/api/space/new", function () {
   global $path;
   $path = "./controllers/APIs/space/new.php";
});

Route("/api/space/get", function () {
   global $path;
   $path = "./controllers/APIs/space/get.php";
});

Route("/api/car/get", function () {
   global $path;
   $path = "./controllers/APIs/car/marca.php";
});
Route("/api/model/get", function () {
   global $path;
   $path = "./controllers/APIs/car/modelo.php";
});



// Disparar o evento
Dispatch();
// print($path);
include($path);
