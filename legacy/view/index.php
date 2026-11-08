<?php
if (!isset($_COOKIE["token"])) header("Location: /login");
else {
   $breakApartToken = explode(".", $_COOKIE["token"]);
   $data = (string)"";
   $id = (string)"";
   $adminCheck = (string)"";
   for ($i = 0; $i < count($breakApartToken); $i++) {
      if ($i == 0) $id =  json_decode(base64_decode($breakApartToken[$i]));

      if ($i == 1) $adminCheck =  json_decode(base64_decode($breakApartToken[$i]));

      if ($i === 2) {
         $data = json_decode(base64_decode($breakApartToken[$i]));
         $data->id === $id ?? header("Location: /login");
         $data->code === $adminCheck ?? header("Location: /login");
      }
   };


   if ($_SERVER["REQUEST_URI"] === "/dashboard" || $_SERVER["REQUEST_URI"] === "/")
      if ((int)$data->code !== 2)
         header("Location: /tickets");
};
?>
<!DOCTYPE html>
<html lang="en">

<head>
   <meta charset="UTF-8">
   <meta http-equiv="X-UA-Compatible" content="IE=edge">
   <meta name="viewport" content="width=device-width, initial-scale=1.0">
   <title>Document</title>
   <link rel="stylesheet" href="public/css/style.css">
   <link rel="stylesheet" href="public/icons/fontawesome-free-6.2.0-web/all.css">
   <link rel="stylesheet" href="public/css/dashboard/dashboard.css">
</head>

<!-- <body> -->

<body class="">
   <div class="drt">
      <header>
         <nav class="nav-bar">
            <h1>Xyami.Park</h1>
            <div class="content-navigation">
               <ul>
                  <li class="active"><a href="/dashboard">Dashboard <div class="line"></div></a></li>
                  <li><a href="/tickets">Tickets <div class="line"></div></a></li>
                  <li><a href="/spaces">Espaços <div class="line"></div></a></li>
                  <!-- Gestão -->
                  <li>
                     <a href="#">Gestão <div class="line"></div></a>
                     <ul class="submenu">
                        <li><a href="/gestor/users">Utilizadores</a></li>
                        <li><a href="/gestor/tickts-story">Histórico de Tickets</a></li>
                        <li><a href="/gestor/table-prices">Tabela de Preços</a></li>
                        <li><a href="/gestor/promotion">Promoções</a></li>
                        <li><a href="/settings">Configuração</a></li>
                     </ul>
                  </li>
               </ul>
            </div>
            <div class="user">
               <div class="picture-user"><img src="./public/images/man.jpg" alt="" srcset=""></div>
               <p><?= $data->nome ?></p>
               <p style="color: #ecb900;"><?= $adminCheck === "2" ? "<i class='fas fa-shield'></i>" : "" ?></p>
            </div>
         </nav>
      </header>
      <p class="route">App / <span>Dashboard</span></p>
      <div class="content-all">
         <div class="cards-content">
            <!--  card -->
            <div class="card-i">
               <div class="item">
                  <p>Faturação</p>
                  <p>0kz</p>
                  <i class="fas fa-money-bill"></i>
               </div>
            </div>
            <!--  card -->
            <div class="card-i">
               <div class="item">
                  <p>Espaço Livres</p>
                  <p>0</p>
                  <i class="fas fa-street-view"></i>
               </div>
            </div>
            <!--  card -->
            <div class="card-i">
               <div class="item">
                  <p>Nº de entradas</p>
                  <p>0</p>
                  <i class="fas fa-door-closed"></i>
               </div>
            </div>
            <!--  card -->
            <div class="card-i">
               <div class="item">
                  <p>Nº de saídas</p>
                  <p>0</p>
                  <i class="fas fa-door-open"></i>
               </div>
            </div>
            <!--  card -->
            <div class="card-i">
               <div class="item">
                  <p>Média total de permanência</p>
                  <p>0kz</p>
                  <i class="fas fa-money-check"></i>
               </div>
            </div>
            <!--  card -->
            <!-- <div class="card-i">
               <div class="item">
                  <p>Quebra de Faturação recorde</p>
                  <p>0kz</p>
                  <i class="fas fa-medal"></i>
               </div>
            </div> -->
         </div>
         <div class="my-chart">
            <div class="content">
               <div class="chart-bars">
                  <?php
                  for ($i = 0; $i <= 100; $i += 10) :
                  ?>
                     <div class="recip-bar" style="height: <?= $i ?>%;">
                        <p class="percentage"><?= $i . "%" ?></p>
                     </div>
                  <?php endfor; ?>
               </div>
               <div class="degree-lines">
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
                  <div class="line-chart"></div>
               </div>
            </div>
         </div>
         <!-- <div class="dashboard-chart">
            <canvas id="chartContentSemanal"></canvas>
            <canvas id="chartContentMensal"></canvas>
         </div> -->
      </div>
      <div class="footer">
         <div class="set-dark-theme">
            <div class="activing"></div>
            <button type="button" class="sun">
               <i class="fa fa-sun"></i>
            </button>
            <button type="button" class="moon">
               <i class="fa fa-moon"></i>
            </button>
         </div>
         <div class="disconnect">
            <button type="button">Desconectar <i class="fas fa-plug"></i></button>
         </div>
      </div>
   </div>
   <script src="public/libs/chartjs/chart.js"></script>
   <script src="public/js/app.js"></script>
   <script src="public/js/index/index.js" type="module"></script>
</body>

</html>