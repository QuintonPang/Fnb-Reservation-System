import React from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import { ChakraProvider } from "@chakra-ui/react";
import QueueForm from "./QueueForm";
import QueueStatus from "./QueueStatus"; // Create a simple page for status display

const App = () => (
  <ChakraProvider>
    <Router>
      <Routes>
        <Route path="/" element={<QueueForm />} />
        <Route path="/queue-status" element={<QueueStatus />} />
      </Routes>
    </Router>
  </ChakraProvider>
);

export default App;
