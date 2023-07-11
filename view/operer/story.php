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


   // if ($_SERVER["REQUEST_URI"] !== "/dashboard" || $_SERVER["REQUEST_URI"] !== "/")
   //    if ((int)$data->code !== 2)
   //       header("Location: /tickets");
};
?>
<!DOCTYPE html>
<html lang="en">

<head>
   <meta charset="UTF-8">
   <meta http-equiv="X-UA-Compatible" content="IE=edge">
   <meta name="viewport" content="width=device-width, initial-scale=1.0">
   <title>Document</title>
   <link rel="stylesheet" href="../public/css/style.css">
   <link rel="stylesheet" href="../public/css/story/story.css">
   <link rel="stylesheet" href="../public/icons/fontawesome-free-6.2.0-web/all.css">
</head>

<!-- <body> -->

<body class="">
   <div class="drt">
      <header>
         <nav class="nav-bar">
            <h1>Xyami.Park</h1>
            <div class="content-navigation">
               <ul>
                  <?=
                  ((int)$data->code === 2) ? "<li><a href='/dashboard'>Dashboard <div class='line'></div></a></li>" : "" ?>
                  <li><a href="/tickets">Tickets <div class="line"></div></a></li>
                  <li><a href="/spaces">Espaços <div class="line"></div></a></li>
                  <!-- Gestão -->
                  <li class="active">
                     <a href="#">Gestão <div class="line"></div></a>
                     <ul class="submenu">
                        <li><a href="/gestor/users">Utilizadores</a></li>
                        <li class="active"><a href="/gestor/tickts-story">Histórico de Tickets</a></li>
                        <li><a href="/gestor/table-prices">Tabela de Preços</a></li>
                        <li><a href="/gestor/promotion">Promoções</a></li>
                        <li><a href="/settings">Configuração</a></li>
                     </ul>
                  </li>
               </ul>
            </div>
            <div class="user">
               <div class="picture-user"><img src="./../public/images/man.jpg" alt="" srcset=""></div>
               <p><?= $data->nome ?></p>
               <p style="color: #ecb900;"><?= $adminCheck === "2" ? "<i class='fas fa-shield'></i>" : "" ?></p>
            </div>
         </nav>
      </header>
      <p class="route">App / Gestor / <span>Histórico de Tickets</span></p>
      <div class="content-all">
         <form action="#">
            <div class="more-field">
               <div class="field">
                  <input type="text" name="consumer_name" placeholder="Nome do Consumidor">
               </div>
               <div class="field">
                  <input type="date" name="date_entrace" placeholder="Data de entrada">
               </div>
               <div class="field">
                  <input type="date" name="date_outside" placeholder="Data de Saída">
               </div>
               <div class="field">
                  <input type="text" name="bi" placeholder="Bilhete de Identidade">
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
               <div class="field">
                  <input type="text" name="code" placeholder="Código do espaço">
               </div>
               <div class="field">
                  <select name="order" id="modelo">
                     <option value="ASC">Order Por</option>
                     <option value="ASC">Ascendente</option>
                     <option value="DESC">Descendente</option>
                  </select>
               </div>
            </div>
            <div class="more-field">
               <div class="field">
                  <input type="text" name="plac" placeholder="O número da matrícula">
               </div>
            </div>
            <div class="more-field">
               <div class="field"></div>
               <div class="field"></div>
               <div class="field"></div>
               <div class="field"></div>
               <div class="field"><button type="button">Pesquisar</button></div>
            </div>
            <div class="table-ticket">
               <div class="head-table">
                  <div class="t-title">
                     <h2>Tabela de controlo de Ticket</h2>
                     <p>Filtrat todos ticket segundo as definições a baixo</p>
                  </div>
               </div>
               <div class="sub-head">
                  <div class="more-field-fake">
                     <div class="field">
                        <select name="order_by" id="">
                           <option value="c.nome">Order por</option>
                           <option value="c.nome">Nome do Ocupante</option>
                           <option value="e.nome">Nome do Espaço</option>
                           <option value="ma.nome">Marca</option>
                           <option value="mo.nome">Modelo</option>
                           <option value="c.matricula_carro">Matrícula</option>
                           <option value="c.data_hora_entrada">Data de entrada</option>
                           <option value="c.data_hora_saida">Data de Saída</option>
                           <option value="total">Total</option>
                           <option value="minuteOcuped">Tempo ocupado</option>
                        </select>
                     </div>
                  </div>
               </div>
               <table>
                  <thead>
                     <tr>
                        <td>#</td>
                        <td>Nome do Ocupante</td>
                        <td>Bilhete</td>
                        <td>Espaço</td>
                        <td>Marca</td>
                        <td>Modelo</td>
                        <td>Matrícula</td>
                        <td>Data de Entrada</td>
                        <td>Data de saída</td>
                        <td>Tempo ocupado</td>
                        <td>Total</td>
                     </tr>
                  </thead>
                  <tbody>
                     <tr>
                        <td>1</td>
                        <td>12341</td>
                        <td>12341</td>
                        <td>3.000kz</td>
                        <td>Hyundia Creta</td>
                        <td>Hyundia Creta</td>
                        <td>Hyundia Creta</td>
                        <td>LD-49-45-10A</td>
                        <td>01 jan 2023</td>
                     </tr>
                  </tbody>
               </table>
            </div>
         </form>
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
   <script src="../public/js/app.js"></script>
   <script type="module" src="../public/js/story/story.js"></script>
</body>

</html>