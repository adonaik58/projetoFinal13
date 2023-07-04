<?php

unset($_COOKIE['token']);
setcookie("token", "", time() - 300, "/");
if (!isset($_COOKIE['token'])) {
    header("Location: /login");
} else {
    header("Location: /");
}
