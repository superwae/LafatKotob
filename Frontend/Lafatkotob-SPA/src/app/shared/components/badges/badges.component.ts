import { CommonModule } from '@angular/common';
import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { BadgesModel } from '../../Models/BadgesModel';
import { TooltipDirective } from '../../directives/tooltip.directive';

@Component({
  selector: 'app-badges',
  standalone: true,
  imports: [CommonModule, TooltipDirective],
  templateUrl: './badges.component.html',
  styleUrls: ['./badges.component.css'],
})
export class BadgesComponent implements OnInit, OnChanges {
  @Input() badges!: BadgesModel[];
  filteredBadges: BadgesModel[] = [];

  constructor() {}

  ngOnInit(): void {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['badges']) {
      this.filteredBadges = this.filterHighestLevelBadges(this.badges);
    }
  }

  filterHighestLevelBadges(badges: BadgesModel[]): BadgesModel[] {
    const highestLevelBadges: { [key: string]: BadgesModel } = {};

    badges.forEach((badge) => {
      const category = this.extractCategory(badge.badgeName);
      const level = this.extractLevel(badge.badgeName);

      if (
        !highestLevelBadges[category] ||
        this.isHigherLevel(
          level,
          this.extractLevel(highestLevelBadges[category].badgeName),
        )
      ) {
        highestLevelBadges[category] = badge;
      }
    });

    return Object.values(highestLevelBadges);
  }

  extractCategory(badgeName: string): string {
    return badgeName.replace(/Bronze|Silver|Gold/, '').trim();
  }

  extractLevel(badgeName: string): string {
    if (badgeName.includes('Gold')) return 'Gold';
    if (badgeName.includes('Silver')) return 'Silver';
    if (badgeName.includes('Bronze')) return 'Bronze';
    return '';
  }

  isHigherLevel(level1: string, level2: string): boolean {
    const levels = ['Bronze', 'Silver', 'Gold'];
    return levels.indexOf(level1) > levels.indexOf(level2);
  }

  onHover(isHovered: boolean) {}
}
