<?php 
    include './pdo.php';

    $jsonEcho = new stdClass();

    //POST DATA
    //characterID: 검색을 요청한 캐릭터의 ID
    //min: 아이템 레벨 제한 최소값
    //max: 아이템 레벨 제한 최대값
    //page: 검색하려고 하는 페이지
    //itemType: 아이템 타입
    //bUserItem: 내 거래 보기인가? 거래소 아이템 보기인가?
    //highPriceOrder: 가격 정렬


    $sql = "SELECT b.id, b.name, b.is_weapon, b.job, b.level_limit, b.strong, b.dexility, b.intellect, b.luck, a.price, a.is_sold FROM registered_item a LEFT JOIN `item` b ON a.item_id = b.id ";
    //현재 필터로 검색했을 때 총 페이지가 몇인지를 알아야 페이징을 할 수 있다. 
    $sql2 = "SELECT COUNT(*) FROM registered_item a LEFT JOIN `item` b ON a.item_id = b.id ";

    //예시
    //SELECT b.id, b.name, b.is_weapon, b.job, b.level_limit, b.strong, b.dexility, b.intellect, b.luck, a.price FROM registered_item a LEFT JOIN `item` b ON a.item_id = b.id WHERE b.character_id != :characterID AND :min <= b.level_limit AND b.level_limit <= :max ORDER BY a.price DESC LIMIT :offset10

    //내 거래 보기인가? 거래소 아이템 보기인가?
    if($_POST["bUserItem"] == 1) {
        //내 아이템 보기
        $sql = $sql."WHERE a.character_id = :characterID ";
        $sql2 = $sql2."WHERE a.character_id = :characterID ";
    }
    else {
        //전체 아이템 보기: 내가 올리지 않은, 팔리지 않은 아이템만 보여준다. 
        $sql = $sql."WHERE b.character_id != :characterID AND a.is_sold = 0 ";
        $sql2 = $sql2."WHERE b.character_id != :characterID AND a.is_sold = 0 ";
    }

    //아이템 타입
    if($_POST["itemType"] == 3) {
        $sql = $sql."AND b.is_weapon = 0 ";
        $sql2 = $sql2."AND b.is_weapon = 0 ";
    }
    else if($_POST["itemType"] == 1) {
        $sql = $sql."AND b.is_weapon = 1 AND b.job = 0 ";
        $sql2 = $sql2."AND b.is_weapon = 1 AND b.job = 0 ";
    }
    else if($_POST["itemType"] == 2) {
        $sql = $sql."AND b.is_weapon = 1 AND b.job = 1 ";
        $sql2 = $sql2."AND b.is_weapon = 1 AND b.job = 1 ";
    }

    //레벨 제한
    $sql = $sql."AND :min <= b.level_limit AND b.level_limit <= :max ";
    $sql2 = $sql2."AND :min <= b.level_limit AND b.level_limit <= :max ";

    //가격 정렬
    if($_POST["highPriceOrder"] == 1) {
        $sql = $sql."ORDER BY a.price DESC ";
        $sql2 = $sql2."ORDER BY a.price DESC ";
    }
    else {
        $sql = $sql."ORDER BY a.price ASC ";
        $sql2 = $sql2."ORDER BY a.price ASC ";
    }

    //페이지
    $sql = $sql."LIMIT :offset, 10 ";

    $pre = $con->prepare($sql);
    $characterID = $_POST["characterID"];
    $pre->bindParam(':characterID', $characterID, PDO::PARAM_INT);
    $min = $_POST["min"];
    $pre->bindParam(':min', $min, PDO::PARAM_INT);
    $max = $_POST["max"];
    $pre->bindParam(':max', $max, PDO::PARAM_INT);
    $page = $_POST["page"] - 1;
    $pre->bindParam(':offset', $page, PDO::PARAM_INT);

    if(!$pre->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "sql fail (sql1)";
        $jsonEcho->sql = $sql;
        echo json_encode($jsonEcho);
        return;
    }

    //sql2 실행하기
    $pre2 = $con->prepare($sql2);
    $characterID = $_POST["characterID"];
    $pre2->bindParam(':characterID', $characterID, PDO::PARAM_INT);
    $min = $_POST["min"];
    $pre2->bindParam(':min', $min, PDO::PARAM_INT);
    $max = $_POST["max"];
    $pre2->bindParam(':max', $max, PDO::PARAM_INT);

    if(!$pre2->execute()) {
        $jsonEcho->success = false;
        $jsonEcho->message = "sql fail (sql2)";
        $jsonEcho->sql = $sql;
        echo json_encode($jsonEcho);
        return;
    }

    $result = $pre->fetchAll(PDO::FETCH_ASSOC);
    $result2 = $pre2->fetchAll(PDO::FETCH_ASSOC);
    $postNum = $result2[0]["COUNT(*)"];
    
    $pageNum = intval($postNum / 10);
    $remain = $postNum % 10;
    if($pageNum == 0) {
        $pageNum += 1;
    }
    else if($remain != 0) {
        $pageNum += 1;
    }

    $jsonEcho->success = true;
    $jsonEcho->pageNum = $pageNum;
    $jsonEcho->data = $result;

    echo json_encode($jsonEcho);
?>
