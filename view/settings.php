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
    <link rel="stylesheet" href="public/css/settings/settings.css">
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
                        <li><a href="/dashboard">Dashboard <div class="line"></div></a></li>
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
        <p class="route">App / <span>Configurações</span></p>
        <div class="content-all">
            <div class="tab-content">
                <div class="header-tab">
                    <div class="head-tab">
                        <div class="tabLine"></div>
                        <button class="active">Sistema</button>
                        <!-- <button>Permissões</button> -->
                        <button></button>
                    </div>
                    <!-- <button></button>
                    <button></button> -->
                </div>
                <div class="tab-appear">
                    <div class="tab">
                        <h2>Configurar Sistema</h2>
                        <br>
                        <form action="#">
                            <div class="more-field">
                                <div class="field">
                                    <label for="">Renda por minuto Ex: 10</label>
                                    <input type="text" name="rend_min" placeholder="Renda por minuto Ex: 10">
                                </div>
                                <div class="field">
                                    <label for="">Número de hora grátis no Espaço</label>
                                    <input type="text" name="num_hour_free" placeholder="Número de hora grátis no Espaço">
                                </div>
                                <div class="field">
                                    <label for="">Quantidade máximo de Espaços</label>
                                    <input type="text" name="quant_max_space" placeholder="Quantidade máximo de Espaços">
                                </div>
                            </div>
                            <div class="more-field">
                                <div class="field">
                                    <button class="save" type="button">Finalizar</button>
                                </div>
                            </div>
                        </form>
                        <!---
                        Renda por hora
                        quanto tempo grátis
                        Limite de espaço
                        -->
                    </div>
                    <!-- <div class="tab">2</div> -->
                    <!-- <div class="tab">3</div>
                    <div class="tab">4</div> -->
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
    <script src="public/libs/chartjs/chart.js"></script>
    <script src="public/js/app.js"></script>
    <script src="public/js/settings/settings.js" type="module"></script>
</body>

</html>