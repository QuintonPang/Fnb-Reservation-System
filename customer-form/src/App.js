import React from "react";
import { BrowserRouter as Router, Route, Routes, Link } from "react-router-dom";
import { ChakraProvider, Button } from "@chakra-ui/react";
import QueueForm from "./QueueForm";
import QueueStatus from "./QueueStatus"; // Create a simple page for status display
import ReservationForm from "./ReservationForm";
import CancelReservationPage from "./CancelReservationPage";
import { Menu, MenuButton, MenuList, MenuItem } from '@chakra-ui/react';
import { ChevronDownIcon } from '@chakra-ui/icons';


function SimpleMenu() {
  return (
    <Menu>
      <MenuButton as={Button} rightIcon={<ChevronDownIcon />}>
        Options
      </MenuButton>
      <MenuList>
        <MenuItem as={Link} to="/">Queue Form</MenuItem>
        <MenuItem as={Link} to="/queue-status">Queue Status</MenuItem>
        <MenuItem as={Link} to="/reservation">Reservation Form</MenuItem>
        <MenuItem as={Link} to="/cancel-reservation">Cancel Reservation</MenuItem>
      </MenuList>
    </Menu>
  );
}
const App = () => (
  <ChakraProvider>
    <Router>
    <SimpleMenu />

      <Routes>
        <Route path="/" element={<QueueForm />} />
        <Route path="/queue-status" element={<QueueStatus />} />
        <Route path="/reservation" element={<ReservationForm />} />
        <Route path="/cancel-reservation" element={<CancelReservationPage />} />
      </Routes>
    </Router>
  </ChakraProvider>
);

export default App;
