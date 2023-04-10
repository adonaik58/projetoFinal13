<?php
$object = [];
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


   if ($_SERVER["REQUEST_URI"] !== "/dashboard" || $_SERVER["REQUEST_URI"] !== "/")
      if ($_SERVER["REQUEST_URI"] !== "/spaces")
         if ((int)$data->code !== 2) header("Location: /tickets");
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
   <link rel="stylesheet" href="public/css/space/space.css">
</head>

<!-- <body> -->

<body class="">
   <div class="drt">
      <div class="toast">
         <div class="icon">
            <i class="fas"></i>
         </div>
         <div class="text">
            <p>Mensagem!</p>
         </div>
      </div>
      <header>
         <nav class="nav-bar">
            <h1>Xyami.Park</h1>
            <div class="content-navigation">
               <ul>
                  <?=
                  ((int)$data->code === 2) ? "<li><a href='/dashboard'>Dashboard <div class='line'></div></a></li>" : "" ?>
                  <li><a href="/tickets">Tickets <div class="line"></div></a></li>
                  <li class="active"><a href="/spaces">Espaços <div class="line"></div></a></li>
                  <!-- Gestão -->
                  <li>
                     <a href="#">Gestão <div class="line"></div></a>
                     <ul class="submenu">
                        <li><a href="/gestor/users">Utilizadores</a></li>
                        <li><a href="/gestor/tickts-story">Histórico de Tickets</a></li>
                        <li><a href="/gestor/table-prices">Tabela de Preços</a></li>
                        <li><a href="/gestor/promotion">Promoções</a></li>
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
      <p class="route">App / <span>Espaços</span></p>
      <div class="content-all">
         <div class="panel-insert-car">
            <div class="head-panel">
               <h1>Ocupar espaço</h1>
               <div class="">
                  <button type="button"><i class="fas fa-times"></i></button>
               </div>
            </div>
            <br>
            <br>
            <form action="">
               <div class="more-field">
                  <div class="field">
                     <input type="text" placeholder="Nome do motorista" name="username">
                  </div>
               </div>
               <div class="more-field">
                  <div class="field">
                     <input type="email" placeholder="Bilhete de identidade" name="bi">
                  </div>
               </div>
               <div class="more-field">
                  <div class="field">
                     <input type="date" placeholder="Idade" name="age">
                  </div>
                  <div class="field">
                     <input type="text" placeholder="Valor atual" name="value">
                  </div>
               </div>
               <div class="more-field">
                  <div class="field">
                     <select name="brand" id="marca">
                        <option value="">Escolher a marca</option>
                     </select>
                  </div>
                  <div class="field">
                     <select name="model" id="modelo">
                        <option value="">Escolher o modelo</option>
                     </select>
                  </div>
               </div>
               <div class="more-field">
                  <div class="field">
                     <input type="color" placeholder="Valor atual" name="color">
                  </div>
                  <div class="field">
                     <input type="text" placeholder="Matrícula" name="plac">
                  </div>
                  <!-- <div class="field"> -->
                  <input type="text" placeholder="id space" name="spaceID" hidden>
                  <!-- </div> -->
               </div>
               <div class="more-field">
                  <div class="field">
                     <button type="button">Gravar <i class="fas fa-save"></i></button>
                  </div>
               </div>
            </form>
            <div class="details">
               <ul>
                  <li>
                     <p><strong>Nome:</strong></p>
                     <p>Adonai Kambu</p>
                  </li>
                  <li>
                     <p><strong>Idade:</strong></p>
                     <p>Adonai Kambu</p>
                  </li>
                  <li>
                     <p><strong>Bilhete de identidade:</strong></p>
                     <p>Adonai Kambu</p>
                  </li>
                  <li>
                     <p><strong>Marca:</strong></p>
                     <p>Adonai Kambu</p>
                  </li>
                  <li>
                     <p><strong>Modelo:</strong></p>
                     <p>Adonai Kambu</p>
                  </li>
                  <li>
                     <p><strong>Matrícula:</strong></p>
                     <p>Adonai Kambu</p>
                  </li>
                  <li>
                     <p><strong>Cor:</strong></p>
                     <p>#fff</p>
                  </li>
               </ul>
               <h1>Total: 130193kz</h1>
               <div class="more-field">
                  <div class="field">
                     <button type="button" style="background:#ff0000;" class="close_ticket">Fechar ticket <i class="fas fa-ticket"></i></button>
                  </div>
               </div>
            </div>
         </div>
         <div class="header-spaces">
            <form action="#">
               <div class="more-field">
                  <div class="field">
                     <input type="text" placeholder="Filtrar por nome">
                  </div>
                  <div class="field">
                     <input type="text" placeholder="Filtrar por matrícula">
                  </div>
                  <div class="field">
                     <input type="text" placeholder="Filtrar por código">
                  </div>
                  <div class="field">
                     <select name="" id="">
                        <option>Estado do espaço</option>
                        <option value="1">Livre</option>
                        <option value="2">Ocupado</option>
                        <option value="3">Indisponível</option>
                     </select>
                  </div>
                  <div class="field">
                     <select name="" id="">
                        <option>Ordem</option>
                        <option value="a-z">A-Z</option>
                        <option value="z-a">Z-A</option>
                     </select>
                  </div>
                  <div class="field">
                     <button type="button">Pesquisar <i class="fas fa-search"></i></button>
                  </div>
               </div>
            </form>
            <!-- <button type="button" class="add-space">Novo Espaço <i class="fas fa-circle-plus"></i></button> -->
            <button type="button" class="update-space">Atualizar <i class="fas fa-sync"></i></button>
         </div>
         <div class="spaces-estacion">
            <div class="card">
               <!-- custo -->
               <!-- codigo espaco -->
               <!-- estado 
                     - livre
                     - Em manutençao
                     - Fechado
                  -->
               <p><b></b></p>
               <h1></h1>
               <div class="card-code">
                  <h3></h3>
               </div>
            </div>
         </div>
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
   <script src="public/js/app.js"></script>
   <script src="public/js/space/space.js" type="module"></script>
</body>

</html>