<?php

function Route(string $path,  $callback)
{
   global $routes;

   $routes[$path] = $callback;
}
function Dispatch()
{
   global $routes;
   $uri = explode("?", (string)$_SERVER["REQUEST_URI"]);
   // echo "<pre>";
   // print_r(($uri));
   // echo "<br>";
   // print_r($_SERVER["REQUEST_URI"]);
   $found = false;
   foreach ($routes as $path => $callback) {
      if ($path !== $uri[0]) continue;
      $found = true;
      $callback();
   }
   if ($found === false) {
      $notFoundCallback = $routes["/404"];
      $notFoundCallback();
   };
}
