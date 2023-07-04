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


   if ($_SERVER["REQUEST_URI"] !== "/dashboard" || $_SERVER["REQUEST_URI"] !== "/")
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
   <link rel="stylesheet" href="../public/css/style.css">
   <link rel="stylesheet" href="../public/icons/fontawesome-free-6.2.0-web/all.css">
   <link rel="stylesheet" href="../public/css/user/users.css">
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
                  <li><a href="/spaces">Espaços <div class="line"></div></a></li>
                  <!-- Gestão -->
                  <li class="active">
                     <a href="#">Gestão <div class="line"></div></a>
                     <ul class="submenu">
                        <li class="active"><a href="/gestor/users">Utilizadores</a></li>
                        <li><a href="/gestor/tickts-story">Histórico de Tickets</a></li>
                        <li><a href="/gestor/table-prices">Tabela de Preços</a></li>
                        <li><a href="/gestor/promotion">Promoções</a></li>
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
      <p class="route">App / Gestor / <span>Usuários</span></p>
      <div class="content-all">

         <div class="window-create-user">
            <div class="painel">
               <div class="head-painel">
                  <button type="button"><i class="fas fa-times"></i></button>
               </div>
               <div class="body-painel">
                  <h1>Criar novo utilizador</h1>
                  <form action="">
                     <div class="more-field">
                        <div class="field">
                           <label for="">Perfil</label>
                           <select name="acess" id="">
                              <option value="">Escolher perfil</option>
                              <option value="1">Operador</option>
                              <option value="2">Administrador</option>
                           </select>
                        </div>
                        <div class="field">
                           <label for="">Nome completo</label>
                           <input type="text" placeholder="Nome completo" name="fullName">
                        </div>
                     </div>
                     <div class="more-field">
                        <div class="field">
                           <label for="">Nome de utilizador</label>
                           <input type="text" placeholder="Nome de usuário" name="name">
                        </div>
                        <div class="field new_password">
                           <label for="">Senha nova</label>
                           <input type="password" placeholder="Introduzir a senha" name="new_password">
                        </div>
                        <div class="field">
                           <label for="">Senha</label>
                           <input type="password" placeholder="Introduzir a senha" name="passwordConfirme">
                        </div>
                     </div>
                     <div class="more-field">
                        <div class="field">
                           <button type="button">CONFIRMAR</button>
                        </div>
                     </div>
                  </form>
               </div>
            </div>
         </div>

         <div class="table-ticket">
            <div class="head-table">
               <div class="t-title">
                  <h2>Tabela de controlo de perfis</h2>
                  <p>Controlo e acesso para alterar informações específicas</p>
               </div>
               <div class="more-field">
                  <button type=""><i class="fas fa-plus-circle"></i> Adicionar Utilizador</button>
               </div>
            </div>
            <div class="sub-head">
               <div class="more-field-fake">
                  <div class="field">
                     <select name="" id="">
                        <option value="">Status</option>
                        <option value="">Aberto</option>
                        <option value="">Fechado</option>
                     </select>
                  </div>
                  <div class="field">
                     <select name="" id="">
                        <option value="">Filtrar</option>
                        <option value="">Ver por tempo</option>
                        <option value="">Fechado</option>
                     </select>
                  </div>
               </div>
            </div>
            <table>
               <thead>
                  <tr>
                     <td>#</td>
                     <td>Status</td>
                     <td>Perfil</td>
                     <td>Nome Completo</td>
                     <td>Nome</td>
                     <td>Ação</td>
                  </tr>
               </thead>
               <tbody>
                  <tr>
                     <td>1</td>
                     <td><i class="fas fa-circle"></i></td>
                     <td>Operador</td>
                     <td><?= $data->nome ?></td>
                     <td>adonaikambu@gmail.com</td>
                     <td>
                        <button type="button"><i class="fas fa-pen"></i></button>
                        <button type="button" class="delete-user"><i class="fas fa-trash"></i></button>
                     </td>
                  </tr>
               </tbody>
            </table>
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
   <script src="../public/js/user/user.js" type="module"></script>
   <script src="../public/js/app.js"></script>
</body>

</html>
<!-- 
   sluttygenny 2.0
   milf destinic
   queen tahshar
   indica puss
   real feedme
 -->