import API from "../API/API.js";

const consumer_name = document.querySelector("form input[name='consumer_name'");
const date_entrace = document.querySelector("form input[name='date_entrace'");
const date_outside = document.querySelector("form input[name='date_outside'");
const bi = document.querySelector("form input[name='bi'");
const brand = document.querySelector("form select[name='brand'");
const model = document.querySelector("form select[name='model'");
const code = document.querySelector("form input[name='code'");
const order_by = document.querySelector("form select[name='order_by'");
const order = document.querySelector("form select[name='order'");
const plac = document.querySelector("form input[name='plac'");
const table = document.querySelector("table tbody");
const search = document.querySelector("form button");
const body = document.querySelector("body");

const getData = async () => {
  const data = {
    consumer_name: consumer_name.value,
    date_entrace: date_entrace.value,
    date_outside: date_outside.value,
    bi: bi.value,
    brand: brand.value,
    model: model.value,
    code: code.value,
    order_by: order_by.value,
    order: order.value,
    plac: plac.value,
  };

  const userData = await API.ticket.getTicketStory(data);
  setData(userData);
};
getData();

search.onclick = async () => {
  const data = {
    consumer_name: consumer_name.value,
    date_entrace: date_entrace.value,
    date_outside: date_outside.value,
    bi: bi.value,
    brand: brand.value,
    model: model.value,
    code: code.value,
    order_by: order_by.value,
    order: order.value,
    plac: plac.value,
  };

  const userData = await API.ticket.getTicketStory(data);

  setData(userData);
};
const dateTimeConversion = (dateTime) => {
  if (dateTime != null && dateTime.trim().length > 0) {
    const months = ["Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"];
    const explodeDateTime = dateTime.split(" ");
    const date = explodeDateTime[0].split("-");
    const time = explodeDateTime[1];
    const getMonth = months[+date[1] - 1];

    return `${date[2]} de ${getMonth} ${date[0]} ${time}`;
  } else return dateTime;
};

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
        <td>${dateTimeConversion(kj.entrance_date)}</td>
        <td>${dateTimeConversion(kj.out_date)}</td>
        <td>${temporizador}</td>
        <td>${kj.total}kz</td>
      </tr>`;
  });
  table.innerHTML = ticket;
}
