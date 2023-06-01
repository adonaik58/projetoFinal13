<?php
include "./models/DB/DBcontroller.php";

class Configuration extends DBController {
    public function autoComplete() {
        $result = $this->select("SELECT 
                renda_min AS renda,
                num_hora_gratis AS num,
                quant_max_espaco AS quant
            FROM config");
        // die($result);
        if (count($result) > 0) {
            return (["status" => true, "data" => $result[0]]);
        }
    }
    public function updateConfiguration() {
        $data = (object)(self::$data);
        if (!empty($data->rendMin) && !empty($data->numHourFree) && !empty($data->quantMaxSpace)) {

            $result = (object)$this->update("UPDATE `config` SET `renda_min` = " . (int)$data->rendMin . ", `num_hora_gratis` = " . (int)$data->numHourFree . ", `quant_max_espaco` = " . (int)$data->quantMaxSpace . ";");
            if ($result->status) {
                http_response_code(self::OK);
                return (["status" => true, "message" => "Configuração alterada"]);
            } else {
                http_response_code(self::METHOD_NOT_ALLOWED);
                return (["status" => false, "message" => "Configuração não alterada"]);
            }
        } else {
            http_response_code(self::METHOD_NOT_ALLOWED);
            return (["status" => false, "message" => "Algum campo está vazio, atualize a página"]);
        }
    }
}
