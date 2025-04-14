import React, { useState, useEffect } from 'react';
import { ChakraProvider, Box, FormControl, FormLabel, Input, Textarea, Button, Stack, Select, Checkbox, useToast, Text } from '@chakra-ui/react';
import { useNavigate } from 'react-router-dom'; // Import useNavigate from React Router

const QueueForm = () => {
  const [webSocket, setWebSocket] = useState(null); // WebSocket state

  const [customerName, setCustomerName] = useState('');
  const [contactNumber, setContactNumber] = useState('');
  const [numberOfGuests, setNumberOfGuests] = useState('');
  const [specialRequests, setSpecialRequests] = useState('');
  const [isSeated, setIsSeated] = useState(false);
  const [outletId, setOutletId] = useState('');
  const toast = useToast();
  const navigate = useNavigate(); // Initialize useNavigate  
  
  
  
  // // WebSocket setup
  // useEffect(() => {
  //   const socket = new WebSocket("ws://localhost:5274"); // Your WebSocket server
  //   setWebSocket(socket);

  //   socket.onopen = () => {
  //     console.log("WebSocket connected");
  //   };

  //   socket.onerror = (error) => {
  //     console.error("WebSocket Error: ", error);
  //     toast({
  //       title: "Connection Error",
  //       description: "Could not connect to WebSocket.",
  //       status: "error",
  //       duration: 5000,
  //       isClosable: true,
  //     });
  //   };

  //   // Clean up WebSocket connection when the component unmounts
  //   return () => {
  //     socket.close();
  //   };
  // }, [toast]);
  const handleSubmit = async (e) => {
    e.preventDefault();

    const newQueue = {
      customerName,
      contactNumber,
      numberOfGuests,
      specialRequests,
      isSeated,
      outletId,
    };

    // Simulate submitting the form (You can replace this with an API call)
    console.log('Queue submitted:', newQueue);

    try {
      // Send the queue data to the backend using fetch
      const response = await fetch('http://localhost:5274/api/Queue', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(newQueue),
      });

      if (response.ok) {
        const data = await response.json();
        const id = data.id; // 🎯 Here's your new queue ID
    
    
        toast({
          title: 'Queue Added',
          description: 'Your queue request has been successfully submitted!',
          status: 'success',
          duration: 5000,
          isClosable: true,
        });

        // Reset form after submission
        setCustomerName('');
        setContactNumber('');
        setNumberOfGuests('');
        setSpecialRequests('');
        setIsSeated(false);
        setOutletId('');

        setTimeout(()=>{
          
 // Redirect to QueueStatus page
 navigate(`/queue-status?id=${id}`);
        },1000)
       
      } else {
        throw new Error('Failed to add queue');
      }
    } catch (error) {
      console.error('Error submitting queue:', error);
      toast({
        title: 'Error',
        description: 'Failed to add your queue request. Please try again.',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }

  };
 const [outlets, setOutlets] = useState([]);

  useEffect(() => {
    const fetchOutlets = async () => {
      try {
        const response = await fetch("http://localhost:5274/api/Outlet");
        const data = await response.json();
        setOutlets(data);
      } catch (error) {
        toast({
          title: "Error fetching outlets.",
          description: error.message,
          status: "error",
          duration: 3000,
          isClosable: true,
        });
      }
    };

    fetchOutlets();
  }, []);
  return (
    <ChakraProvider>
      <Box maxW="md" mx="auto" mt="8" p="5" borderWidth="1px" borderRadius="lg">
        <Text mb={4} fontSize={'5xl'} textAlign={'center'} fontWeight={600}> Join Queue</Text>
        <form onSubmit={handleSubmit}>
          <Stack spacing={4}>
            <FormControl id="customerName" isRequired>
              <FormLabel>Customer Name</FormLabel>
              <Input
                type="text"
                value={customerName}
                onChange={(e) => setCustomerName(e.target.value)}
              />
            </FormControl>

            <FormControl id="contactNumber" isRequired>
              <FormLabel>Contact Number</FormLabel>
              <Input
                type="text"
                value={contactNumber}
                onChange={(e) => setContactNumber(e.target.value)}
              />
            </FormControl>

            <FormControl id="numberOfGuests" isRequired>
              <FormLabel>Number of Guests</FormLabel>
              <Input
                type="number"
                value={numberOfGuests}
                onChange={(e) => setNumberOfGuests(e.target.value)}
              />
            </FormControl>

            <FormControl id="specialRequests">
              <FormLabel>Special Requests</FormLabel>
              <Textarea
                value={specialRequests}
                onChange={(e) => setSpecialRequests(e.target.value)}
              />
            </FormControl>

            {/* <FormControl id="isSeated">
              <Checkbox
                isChecked={isSeated}
                onChange={(e) => setIsSeated(e.target.checked)}
              >
                Is Seated?
              </Checkbox>
            </FormControl> */}

               <FormControl isRequired>
                     <FormLabel>Outlet</FormLabel>
                     <Select name="outletId" value={outletId} onChange={(e)=>setOutletId(e.target.value)}>
                       <option value="">Select Outlet</option>
                       {outlets.map((outlet) => (
                         <option key={outlet.id} value={outlet.id}>
                           {outlet.name} - {outlet.location}
                         </option>
                       ))}
                     </Select>
                   </FormControl>
            <Button
              type="submit"
              colorScheme="teal"
              size="lg"
            // isFullWidth
            >
              Join Queue
            </Button>
          </Stack>
        </form>
      </Box>
    </ChakraProvider>
  );
};

export default QueueForm;
