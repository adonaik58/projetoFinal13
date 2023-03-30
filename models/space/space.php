<?php

include "./models/DB/DBcontroller.php";

class Space extends DBController {

   public function insertCartInSpace() {
   }
   public function createSpace() {
      /* var allSpace = [];
      const alphabet = "ABCDEFGHIJKLMNOQRSTUVWXYZ".split("");
      for (var i = 0; i < alphabet.length; i++){
         var t = 1;
         for(; t <= 10; t++){
            allSpace.push(alphabet[i]+t)
         }
      } */

      $allSpace = [];
      $alphabet = explode(".", "A.B.C.D.E.F.G.H.I.J.K.L.M.N.O.P.Q.R.S.T.U.V.W.X.Y.Z");

      for ($i = 0; $i < count($alphabet); $i++) {
         $t = 1;
         for (; $t <= 10; $t++) {
            array_push($allSpace, $alphabet[$i] . $t);
         }
      }
      foreach ($allSpace as $each) {
         $uID = uniqid();
         $this->insertData("INSERT INTO espacos VALUES(default, '{$each}', '{$uID}', 2, 2)");
      }
      echo "<pre>";
      return ($allSpace);
   }
}
