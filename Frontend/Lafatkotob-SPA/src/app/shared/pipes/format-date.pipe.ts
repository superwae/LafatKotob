import { Pipe, PipeTransform } from '@angular/core';
import { format, isSameDay, isSameWeek } from 'date-fns';

@Pipe({
  name: 'formatDate',
  standalone: true  // Make the pipe standalone
})
export class FormatDatePipe implements PipeTransform {
  transform(value: Date | string): string {
    const date = new Date(value);
    const now = new Date();

    // Check if the date is the same day as today
    if (isSameDay(date, now)) {
      return ` ${format(date, 'p')}`;
    }

    // Check if the date is within the same week
    if (isSameWeek(date, now, { weekStartsOn: 1 })) { // weekStartsOn: 1 means the week starts on Monday
      return `${format(date, 'EEE, p')}`;
    }

    // If the date is older than a week
    return `${format(date, 'Pp')}`;
  }
}
