<?php
isset($_COOKIE["token"]) ? header("Location: /") : "";
?>
<!DOCTYPE html>
<html lang="en">

<head>
   <meta charset="UTF-8">
   <meta http-equiv="X-UA-Compatible" content="IE=edge">
   <meta name="viewport" content="width=device-width, initial-scale=1.0">
   <title>Document</title>
   <link rel="stylesheet" href="public/css/login/login.css">
   <link rel="stylesheet" href="public/icons/fontawesome-free-6.2.0-web/all.css">
</head>

<body>
   <header>
      <h1>Xyami.Park</h1>
   </header>

   <!-- 
      id
      code de acesso
      nome
      senha
    -->
   <?php "" . md5("lopes") ?>

   <div class="form-content">

      <div class="toast">
         <div class="icon">
            <i class="fas"></i>
         </div>
         <div class="text">
            <p>Mensagem!</p>
         </div>
      </div>
      <!-- <h1>Login</h1> -->
      <br>
      <form action="#">
         <div class="field">
            <input type="text" name="name" placeholder="Seu nome">
         </div>
         <div class="field">
            <input type="password" name="password" autocomplete="FALSE" class="password" placeholder="Inserir a senha">
            <button class="see">Mostrar</button>
         </div>
         <div class="field">
            <button type="button">Continuar</button>
         </div>
      </form>
   </div>
   <script src="./public/js/login/login.js" type="module"></script>
</body>

</html>