<?php
    include './pdo.php';

    $jsonEcho = new stdClass();

    //post data
    //item_id: 대금을 수령할 아이템의 아이디
    //character_id: 대금을 수령할 캐릭터의 아이디

    $sql = "SELECT (SELECT `character_id` FROM item WHERE id = a.item_id) AS character_id, a.price FROM registered_item a WHERE item_id = :item_id AND is_sold = 1";
    $pre = $con->prepare($sql);
    $pre->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);

    //select fail
    if(!$pre->execute())
    {
        $jsonEcho->success = false;
        $jsonEcho->message = "select fail";
        echo json_encode($jsonEcho);
        return;
    }
    
    $count = $pre->rowCount();
    if($count != 1) {
        $jsonEcho->success = false;
        $jsonEcho->message = "no item";
        echo json_encode($jsonEcho);
        return;
    } 

    $result = $pre->fetchAll(PDO::FETCH_ASSOC);
    //아이템을 등록한 캐릭터의 아이디
    $character_id = $_POST["character_id"];
    //대금
    $price = $result[0]["price"];

    //registered_item에서 해당 아이템을 삭제한다
    $sql2 = "DELETE FROM registered_item WHERE `item_id` = :item_id";
    $pre2 = $con->prepare($sql2);
    $pre2->bindParam(":item_id", $_POST["item_id"], PDO::PARAM_INT);
    
    if(!$pre2->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "delete fail";
        echo json_encode($jsonEcho);
        return;
    }

    //character table에서 대금을 수령한 캐릭터의 gold를 대금만큼 추가한다. 
    $sql3  = "UPDATE `character` SET gold = gold + :price WHERE `id` = :character_id";
    $pre3 = $con->prepare($sql3);
    $pre3->bindParam(":price", $price, PDO::PARAM_INT);
    $pre3->bindParam(":character_id", $character_id, PDO::PARAM_INT);

    if(!$pre3->execute()) 
    {
        $jsonEcho->success = false;
        $jsonEcho->message = "update gold fail";
        echo json_encode($jsonEcho);
        return;
    }


    $jsonEcho->success = true;
    $jsonEcho->item_id = $_POST["item_id"];
    $jsonEcho->price = $price;
    echo json_encode($jsonEcho);
    return;
?>