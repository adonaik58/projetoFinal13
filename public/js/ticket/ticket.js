import API from "../API/API.js";

const table = document.querySelector("table tbody");
const filter = document.querySelector(".more-field-fake .field:nth-child(1) select");
const order = document.querySelector(".more-field-fake .field:nth-child(2) select");

filter.onchange = async () => {
  if (filter.value.trim() != "") {
    const userData = await API.ticket.getTicket(filter.value, order.value);
    setData(userData);
  }
};
order.onchange = async () => {
  if (filter.value.trim() != "") {
    const userData = await API.ticket.getTicket(filter.value, order.value);
    setData(userData);
  }
};
async function getTicket() {
  const userData = await API.ticket.getTicket();

  setData(userData);

  // return content.length > 0 ? true : false;
}
getTicket();

function setData(userData) {
  var ticket = "";

  userData.map((kj) => {
    var tempo = kj?.time_Ocuped;
    var temporizador =
      tempo?.year > 0
        ? `${tempo.year} ano/s e ${tempo.month} mês/meses`
        : tempo?.month > 0
        ? `${tempo.month}m e ${tempo.day}d`
        : tempo?.day > 0
        ? `${tempo.day}d e ${tempo.hours}h`
        : tempo?.hours > 0
        ? `${tempo.hours}h e ${tempo.minutes}min `
        : tempo?.minutes > 0
        ? `${tempo.minutes}min e ${tempo.secondes}seg`
        : tempo?.secondes > 0
        ? `${tempo.secondes}seg`
        : "";
    ticket += `<tr>
        <td>${kj.id}</td>
        <td>${kj.name}</td>
        <td>${kj.bi}</td>
        <td>${kj.s_name}</td>
        <td>${kj.brand}</td>
        <td>${kj.model}</td>
        <td>${kj.plac}</td>
        <td>${kj.entrance_date}</td>
        <td>${kj.out_date}</td>
        <td>${temporizador}</td>
        <td>${kj.total}kz</td>
      </tr>`;
  });
  table.innerHTML = ticket;
}
