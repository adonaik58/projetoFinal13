import API from "../API/API.js";

async function getTicket() {
  const userData = await API.ticket.getTicket();

  console.log(userData);

  // return content.length > 0 ? true : false;
}
getTicket();
