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

Route("/tickets", function () {
   global $path;
   $path = "./view/tickets.php";
});
Route("/spaces", function () {
   global $path;
   $path = "./view/spaces.php";
});
Route("/settings", function () {
   global $path;
   $path = "./view/settings.php";
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

Route("/api/space/insert/user", function () {
   global $path;
   $path = "./controllers/APIs/space/insertuser.php";
});

Route("/api/space/get", function () {
   global $path;
   $path = "./controllers/APIs/space/get.php";
});
Route("/api/space/close", function () {
   global $path;
   $path = "./controllers/APIs/space/close.php";
});

Route("/api/car/get", function () {
   global $path;
   $path = "./controllers/APIs/car/marca.php";
});

Route("/api/model/get", function () {
   global $path;
   $path = "./controllers/APIs/car/modelo.php";
});

// Dashoard

Route("/api/dashboard", function () {
   global $path;
   $path = "./controllers/APIs/dashboard/index.php";
});

// Ticket

Route("/api/ticket/get", function () {
   global $path;
   $path = "./controllers/APIs/ticket/get.php";
});

// configuration

Route("/api/config/update", function () {
   global $path;
   $path = "./controllers/APIs/configuration/update.php";
});

Route("/api/config/autocomplete", function () {
   global $path;
   $path = "./controllers/APIs/configuration/autoComplete.php";
});


// Disparar o evento
Dispatch();
// print($path);
include($path);
