<?php

include "./models/DB/DBcontroller.php";


class Dashboard extends DBController {

    public function getDashboardResults(int $var = null) {
        return [
            "totalPayment"      => $this->FacturationService(),
            "spaceOpen"         => $this->FreeSpace(),
            "numCarEntrance"    => $this->carEntrance(),
            "ticketClosed"      => $this->ticketClosed(),
            "spaceOcuped"       => $this->spaceOcuped(),
        ];
    }

    private function FacturationService() {
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



            if ($result->execute()) {
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
                http_response_code(self::OK);
                return ($preco);
            } else {
                http_response_code(self::EXPECTATION_FAILED);
                return (["status" => false, "message" => "Algo deu errado ao pegar o total"]);
            }
        } catch (\Exception) {
            http_response_code(self::EXPECTATION_FAILED);
            return (["status" => false, "message" => "Algo deu errado"]);
        }
    }
    private function FreeSpace() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM espacos e
        WHERE e.estado = 'a'");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function spaceOcuped() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM espacos e
        WHERE e.estado = 'i'");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function carEntrance() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM consumidores");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
    private function ticketClosed() {
        $result = $this->connection->query("SELECT 
        COUNT(*) AS count
        FROM ticket_historico");

        return $result->fetchAll(PDO::FETCH_OBJ)[0]->count;
    }
}
