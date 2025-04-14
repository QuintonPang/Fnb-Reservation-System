import React, { useState } from 'react';
import {
  Box,
  Heading,
  Input,
  Button,
  VStack,
  useToast,
  FormControl,
  FormLabel,
} from '@chakra-ui/react';

const CancelReservationPage = () => {
  const [id, setId] = useState('');
  const [contactNumber, setContactNumber] = useState('');
  const toast = useToast();

  const handleCancel = async () => {
    try {
        const response = await fetch(`http://localhost:5274/api/Reservation/cancel`, {
            method: 'POST',
            headers: {
              'Content-Type': 'application/json',
            },
            body: JSON.stringify({
              id, // make sure this is a number
              contactNumber, // make sure this is a string
            }),
          });
          

          if(response.ok){

              toast({
                title: 'Reservation Cancelled',
                description: 'Successfully cancelled your reservation.',
                status: 'success',
                duration: 5000,
                isClosable: true,
              });
              setId('');
              setContactNumber('');
          }else{

            toast({
                title: 'Error',
                description:  response?.statusText,
                status: 'error',
                duration: 5000,
                isClosable: true,
              });
          }

    } catch (error) {
        console.log(error)
      toast({
        title: 'Error',
        description:  'Something went wrong.',
        status: 'error',
        duration: 5000,
        isClosable: true,
      });
    }
  };

  return (
    <Box maxW="md" mx="auto" mt={10} p={6} borderWidth={1} borderRadius="lg" boxShadow="lg">
      <Heading mb={6} size="lg" textAlign="center">
        Cancel Reservation
      </Heading>

      <VStack spacing={4}>
        <FormControl>
          <FormLabel>Reservation ID</FormLabel>
          <Input
            type="number"
            value={id}
            onChange={(e) => setId(e.target.value)}
            placeholder="Enter Reservation ID"
          />
        </FormControl>

        <FormControl>
          <FormLabel>Contact Number</FormLabel>
          <Input
            type="tel"
            value={contactNumber}
            onChange={(e) => setContactNumber(e.target.value)}
            placeholder="Enter Contact Number"
          />
        </FormControl>

        <Button colorScheme="red" width="full" onClick={handleCancel}>
          Cancel Reservation
        </Button>
      </VStack>
    </Box>
  );
};

export default CancelReservationPage;
