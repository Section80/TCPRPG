<?php
    include './pdo.php';

    $jsonEcho = new stdClass();

    //post data
    //character_id: 아이템을 등록하는 캐릭터의 id
    //item_id: 등록할 아이템의 아이디
    //price: 가격

    //해당 아이템의 index_in_inventory 를 -1로 바꾼다. 
    $sql = "UPDATE item SET `index_in_inventory` = -1 WHERE `id` = :item_id";
    $pre = $con->prepare($sql);
    $pre->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    if(!$pre->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "update item table fail";
        echo json_encode($jsonEcho);
        return;
    }

    //registered_item에 insert한다
    $sql2 = "INSERT INTO registered_item(`character_id`, `item_id`, `price`) VALUES(:character_id, :item_id, :price)";
    $pre2 = $con->prepare($sql2);
    $pre2->bindParam(":character_id", $_POST["character_id"], PDO::PARAM_INT);
    $pre2->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);
    $pre2->bindParam(":price", $_POST["price"], PDO::PARAM_INT);

    if(!$pre2->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "update registered_item table fail";
        echo json_encode($jsonEcho);
        return;
    }

    $jsonEcho->success = true;
    $jsonEcho->item_id = $_POST["item_id"];
    echo json_encode($jsonEcho);
?>