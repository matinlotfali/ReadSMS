<?

function Post($method, $data)
{
    $authenticationToken = "724126539:AAFdJRDjZuldgM2Ny8PXKD4iH6ekrp8Lbws";
    $url = "https://api.telegram.org/bot".$authenticationToken."/".$method;
    //$url = "http://149.154.167.200/bot".$authenticationToken."/".$method;
    $curl = curl_init();
    curl_setopt($curl, CURLOPT_RETURNTRANSFER, true);
    curl_setopt($curl, CURLOPT_CUSTOMREQUEST, "POST");
    curl_setopt($curl, CURLOPT_POST, true);
    curl_setopt($curl, CURLOPT_POSTFIELDS, json_encode($data));
    curl_setopt($curl, CURLOPT_HTTPHEADER, array("Content-type: application/json"));
    curl_setopt($curl, CURLOPT_URL, $url);
    curl_setopt($curl, CURLOPT_ENCODING,  '');
    $resultJSON = curl_exec($curl);
    
    $result = json_decode($resultJSON,true);
    if(!$result["ok"])
        LogF($resultJSON);
    curl_close($curl);
    return $result;
}

$input = file_get_contents('php://input');

$fp = fopen('data.txt', 'a');
fwrite($fp, $input);
fwrite($fp, "<br>\n");
fclose($fp);

$json = json_decode($input, true);

foreach ($json as $item)
{
    $data = array(
                "chat_id" => "97306666",
                "text" => 
        "Message from ".$item["from"].
        " on ". date("Y-m-d H:i:s",$item["date"]-14400).
        ":\n". $item["body"] 
    );
    Post("sendMessage", $data);
}
?>