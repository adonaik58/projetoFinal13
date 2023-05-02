<?php

include "./models/DB/DBcontroller.php";


class Dashboard extends DBController {

    // public function __construct(int $var = null) {
    //     return [
    //         $this->FacturationService()
    //     ];
    // }

    public function FacturationService() {
        try {

            // if (is_null($this->connection)) {
            //     http_response_code(self::EXPECTATION_FAILED);
            //     return json_encode(["status" => false, "message" => "Não há conexão"]);
            // }
            $result = $this->connection->query('SELECT 
                c.data_entrada as i,
                c.data_saida as f
                FROM ticket_historico c
            ');

            $preco = 0;
            foreach ($result->fetchAll(PDO::FETCH_OBJ) as $row) {

                // Calculando o intervalo de tempo
                $data_passada = new DateTime($row->i ?? "");
                $data_atual   = new DateTime($row->f ?? "");
                $intervalo    = $data_atual->diff($data_passada);


                $year    = $intervalo->format('%y');
                $month   = $intervalo->format('%m');
                $day     = $intervalo->format('%d');
                $hour    = $intervalo->format('%h');
                $min     = $intervalo->format('%i');
                $seg     = $intervalo->format('%s');

                // calculando o preço
                $preco += (((((((($year * 12) + $month) * 31) + $day) * 24) + $hour) * 60) + $min) * 10;
            }


            if ($result->execute()) {
                http_response_code(self::OK);
                return json_encode($preco);
            } else {
                http_response_code(self::EXPECTATION_FAILED);
                return json_encode(["status" => false, "message" => "Algo deu errado ao pegar o total"]);
            }
        } catch (\Exception) {
            http_response_code(self::EXPECTATION_FAILED);
            return json_encode(["status" => false, "message" => "Algo deu errado"]);
        }
    }
}
