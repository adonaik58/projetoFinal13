<?php
include("./controllers/isAuth.php")
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
   <link rel="stylesheet" href="public/css/ticket/ticket.css">
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
                  <li class="active"><a href="/tickets">Tickets <div class="line"></div></a></li>
                  <li><a href="/spaces">Espaços <div class="line"></div></a></li>
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
      <p class="route">App / <span>Tickets</span></p>
      <div class="content-all">
         <div class="modal-detail">
            <div class="back-modal">
               <div class="modal-s">
                  <div class="head">
                     <div class="head-flex">
                        <p>Adonai Kambu</p>
                        <h2>A4</h2>
                     </div>
                  </div>
                  <div class="body">
                     <div class="body-content">
                        <div class="item">
                           <p>Marca do carro</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Modelo do carro</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Cor</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Matrícula</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Data de entrada</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Data de saída</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Tempo ocupado</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <p>Desconto</p>
                           <p>Buggy</p>
                        </div>
                        <div class="item">
                           <h2>Total</h2>
                           <p>0kz</p>
                        </div>
                     </div>
                  </div>
                  <div class="footer">
                     <div class="footer-content">
                        <div class="field">
                           <button type="button">OK</button>
                        </div>
                     </div>
                  </div>
               </div>
            </div>
         </div>
         <form action="">
            <div class="table-ticket">
               <div class="head-table">
                  <div class="t-title">
                     <h2>Tabela de controlor Ticket</h2>
                     <p>Filtrat todos ticket segundo as definições a baixo</p>
                  </div>
               </div>
               <div class="sub-head">
                  <div class="more-field-fake">
                     <div class="field">
                        <select name="" id="">
                           <option value="">Filtrar</option>
                           <option value="e.nome">Nome do Espaço</option>
                           <option value="c.nome">Nome do Ocupante</option>
                           <option value="ma.nome">Marca</option>
                           <option value="mo.nome">Modelo</option>
                           <option value="c.matricula_carro">Matrícula</option>
                           <option value="c.data_hora_entrada">Data de entrada</option>
                           <option value="c.data_hora_saida">Data de Saída</option>
                           <option value="total">Total</option>
                           <option value="minuteOcuped">Tempo ocupado</option>
                        </select>
                     </div>
                     <div class="field">
                        <select name="" id="">
                           <option value="ASC">Ordem</option>
                           <option value="ASC">A-Z</option>
                           <option value="DESC">Z-A</option>
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
   <script src="public/js/app.js"></script>
   <script src="public/js/ticket/ticket.js" type="module"></script>
</body>

</html>