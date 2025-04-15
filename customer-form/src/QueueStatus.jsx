import React, { useEffect, useState } from "react";
import {
  Box,
  Text,
  useToast,
  Input,
  Button,
  VStack,
  Heading,
} from "@chakra-ui/react";
import { useLocation } from "react-router-dom";

const QueueStatus = () => {
  const location = useLocation();
  const queryParams = new URLSearchParams(location.search);
  const id = queryParams.get('id'); // date from the query parameter
 
  const [webSocket, setWebSocket] = useState(null);
  const [queue, setQueue] = useState(null);
  const [queueId, setQueueId] = useState(id);
  const [peopleAhead, setPeopleAhead] = useState(null);
  const toast = useToast();


  useEffect(() => {
    const socket = new WebSocket("ws://localhost:5274/queue-updates");
    setWebSocket(socket);

    socket.onopen = () => console.log("WebSocket connected");

    socket.onmessage = (event) => {
      const data = JSON.parse(event.data);
      setQueue(data);
    };

    socket.onerror = (error) => {
      console.error("WebSocket Error: ", error);
      toast({
        title: "Connection Error",
        description: "Could not connect to WebSocket.",
        status: "error",
        duration: 5000,
        isClosable: true,
      });
    };

    socket.onclose = () => console.log("WebSocket connection closed");

    return () => socket.close();
  }, [toast]);

// Function to find the current queue and count people ahead
function countPeopleAhead(queueData, currentQueueId) {
  // Find the current queue based on queueId
  const currentQueue = queueData.find(item => item.Queue.Id.toString() === currentQueueId);

  
  if (!currentQueue) {
    toast({
      title: "Not Found",
      description: "Queue ID not found.",
      status: "warning",
      duration: 4000,
      isClosable: true,
    });
    setPeopleAhead(null);
    return null;
   
  }
  const myTime = new Date(currentQueue.Queue.DateTime);

  const currentTableIds = currentQueue.TableIds; // Get all tableIds for the current queue
  const currentOutletId = currentQueue.Queue.outletId; // Get outletId
  let totalPeopleAhead = 0;

  // Iterate through the queue data and count people ahead in the same outlet
  for (const item of queueData) {
    // Skip if it's the current queue
    // if (item.Queue.Id === currentQueueId) {
    //   break;  // Exit the loop when we reach the current queue
    // }
    // Count people ahead in the same outlet and matching tableIds
    if (item.Queue.outletId === currentOutletId && item.TableIds.some(tableId => currentTableIds.includes(tableId))     &&   new Date(item?.Queue?.DateTime).getTime() < myTime.getTime()
    ) {
  console.log("DONE")
      totalPeopleAhead += 1;
    }
  }

  return totalPeopleAhead;
}
  useEffect(()=>{
    if(!id||!queue) return;
    console.log(queue)
    checkQueue(id)
  },[id,queue])
  const checkQueue = (id) => {

  
    const aheadCount = countPeopleAhead(queue, id);

    setPeopleAhead(aheadCount);
  };

  return (
    <Box p={6} textAlign="center">
      <Heading size="lg" mb={4}>Check Your Queue Status</Heading>
      <VStack spacing={4}>
        <Input
          placeholder="Enter your Queue ID"
          value={queueId}
          onChange={(e) => setQueueId(e.target.value)}
          maxW="300px"
        />
        <Button colorScheme="blue" onClick={()=>checkQueue(queueId)}>
          Check Queue
        </Button>
        {peopleAhead !== null&& queue && (
          <Box mt={6}>
           {peopleAhead!==0? <><Text fontSize="xl">
              You're in the queue! There {peopleAhead === 1 ? "is" : "are"}{" "}
              {peopleAhead} group{peopleAhead !== 1 && "s"} of people ahead of you.
            </Text>
            <Text>The staff will notify you when your table is ready.</Text>
            </>
          
          :<Text fontSize="xl">Your table is ready! Please be at the counter ASAP!</Text>}
          </Box>
        )}
      </VStack>
    </Box>
  );
};

export default QueueStatus;
