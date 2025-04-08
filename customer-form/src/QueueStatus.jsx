import React from "react";
import { Box, Text } from "@chakra-ui/react";

const QueueStatus = () => {
  return (
    <Box p={5} textAlign="center">
      <Text fontSize="2xl" mb={3}>
        You're in the queue!
      </Text>
      <Text>The staff will notify you when your table is ready.</Text>
    </Box>
  );
};

export default QueueStatus;
