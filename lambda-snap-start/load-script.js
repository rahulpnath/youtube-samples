import http from "k6/http";

export const options = {
  scenarios: {
    constant_rate: {
      executor: "constant-arrival-rate",
      rate: 10, // Total requests
      timeUnit: "3s", // Spread over 3 seconds
      duration: "3s", // Total duration
      preAllocatedVUs: 10, // Number of virtual users
    },
  },
};

export default function () {
  http.get(
    "https://za3pxv4on3rzgmo4qscfaopgly0wpata.lambda-url.ap-southeast-2.on.aws/"
  );
}
