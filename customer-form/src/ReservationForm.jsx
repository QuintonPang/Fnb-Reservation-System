import React, { useState, useEffect } from "react";
import {
  Box,
  Button,
  FormControl,
  FormLabel,
  Input,
  Select,
  Stack,
  NumberInput,
  NumberInputField,
  useToast,
  Text,
} from "@chakra-ui/react";
import dayjs from "dayjs";

const ReservationForm = () => {


  const toast = useToast();


  const [formData, setFormData] = useState({
    customerName: "",
    contactNumber: "",
    reservationDate: dayjs().format("YYYY-MM-DD"),
    reservationTime: "12:00",
    numberOfGuests: 1,
    outletId: "",
  });

  const [outlets, setOutlets] = useState([]);
  const timeOptions = Array.from({ length: 24 }, (_, i) =>
    `${i.toString().padStart(2, "0")}:00`
  );


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

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    const reservationDateTime = `${formData.reservationDate}T${formData.reservationTime}`;

    const payload = {
      customerName: formData.customerName,
      contactNumber: formData.contactNumber,
      reservationDateTime,
      numberOfGuests: Number(formData.numberOfGuests),
      status: "Pending",
      outletId: (formData.outletId),
    };

    try {
      const res = await fetch("http://localhost:5274/api/Reservation", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      if (res.ok) {
        toast({
          title: "Reservation added. ID is "  + (await res.json()).id,
          status: "success",
          duration: 7000,
          isClosable: true,
        });
        // Optionally reset form
      } else {
        throw new Error("Failed to submit reservation.");
      }
    } catch (err) {
      toast({
        title: "Error",
        description: err.message,
        status: "error",
        duration: 3000,
        isClosable: true,
      });
    }
  };

  return (
    <Box maxW="md" mx="auto" mt={10} p={6} borderWidth={1} borderRadius="lg">
      <Text mb={4} fontSize={'5xl'} textAlign={'center'} fontWeight={600}> Table Reservation</Text>

      <form onSubmit={handleSubmit}>
        <Stack spacing={4}>
          <FormControl isRequired>
            <FormLabel>Customer Name</FormLabel>
            <Input name="customerName" value={formData.customerName} onChange={handleChange} />
          </FormControl>

          <FormControl isRequired>
            <FormLabel>Contact Number</FormLabel>
            <Input name="contactNumber" value={formData.contactNumber} onChange={handleChange} />
          </FormControl>

          <FormControl isRequired>
            <FormLabel>Reservation Date</FormLabel>
            <Input type="date" name="reservationDate" value={formData.reservationDate} onChange={handleChange} />
          </FormControl>

          <FormControl isRequired>
            <FormLabel>Reservation Time</FormLabel>
            <Select name="reservationTime" value={formData.reservationTime} onChange={handleChange}>
              {timeOptions.map((time) => (
                <option key={time} value={time}>
                  {time}
                </option>
              ))}
            </Select>
          </FormControl>

          <FormControl isRequired>
            <FormLabel>Number of Guests</FormLabel>
            <NumberInput min={1} value={formData.numberOfGuests} onChange={(valueString) => setFormData({ ...formData, numberOfGuests: valueString })}>
              <NumberInputField name="numberOfGuests" />
            </NumberInput>
          </FormControl>


          <FormControl isRequired>
            <FormLabel>Outlet</FormLabel>
            <Select name="outletId" value={formData.outletId}  onChange={(e ) => setFormData({ ...formData, outletId: e.target.value })}>
              <option value="">Select Outlet</option>
              {outlets.map((outlet) => (
                <option key={outlet.id} value={outlet.id}>
                  {outlet.name} - {outlet.location}
                </option>
              ))}
            </Select>
          </FormControl>



          <Button type="submit" colorScheme="blue">
            Submit Reservation
          </Button>
        </Stack>
      </form>
    </Box>
  );
};

export default ReservationForm;
