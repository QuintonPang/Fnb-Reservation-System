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


  useEffect(()=>{
    if(!id||!queue) return;
    console.log(queue)
    checkQueue(id)
  },[id,queue])
  const checkQueue = (id) => {
    const myQueue =Array.isArray(queue)&& queue.find((q) => q.Id?.toString() === id.toString());

    if (!myQueue) {
      toast({
        title: "Not Found",
        description: "Queue ID not found.",
        status: "warning",
        duration: 4000,
        isClosable: true,
      });
      setPeopleAhead(null);
      return;
    }
    

    const myTime = new Date(myQueue.DateTime);
    const outletId = myQueue.outletId?.toString();

    const aheadCount = queue.filter(
      (q) =>
        q.outletId?.toString() === outletId &&
        new Date(q.DateTime).getTime() < myTime.getTime()
    ).length;

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
